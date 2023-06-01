using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpanByteExtenders;

namespace FileContainer;

static class PagedContainerEntryExtenders
{
    static readonly DateTime DT_FROM = new(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    internal static Dictionary<string, PagedContainerEntry> ReadEntries(this byte[] buff, IDataPacker handler)
    {
        var r = new Dictionary<string, PagedContainerEntry>(StringComparer.InvariantCultureIgnoreCase);

        if (buff.Length == 0)
            return r;

        var span = handler.Unpack(buff);

        var count = span.Read<int>();
        for (var i = 0; i < count; i++)
        {
            var firstPage        = span.Read<int>();
            var lastPage         = span.Read<int>();
            var rawLength        = span.Read<int>();
            var compressedLength = span.Read<int>();
            if (compressedLength == 0)
                rawLength = compressedLength;

            var modified = span.Read<int>();
            var name     = span.ReadPrefixedString(ReadStringPrefix.Short);
            var flags    = (EntryFlags) span.Read<byte>();

            r.Add(name, new PagedContainerEntry(name, firstPage, lastPage, rawLength, compressedLength, flags, DT_FROM.AddSeconds(modified)));
        }

        return r;
    }

    internal static byte[] WriteEntries(this Dictionary<string, PagedContainerEntry> entries, IDataPacker handler)
    {
        var requiredBufferLength = 4 + (entries.Any()
                                            ? entries.Sum(c => 4                                              + 4 + 4 + 4 + 4 +
                                                               (2 + Encoding.UTF8.GetByteCount(c.Value.Name)) +
                                                               1)
                                            : 0);
        var buff = new byte[requiredBufferLength];

        var span = buff.AsSpan();
        span.Write(entries.Count);

        foreach (var kvp in entries)
        {
            var entry = kvp.Value;

            span.Write(entry.FirstPage);        // 4 byte
            span.Write(entry.LastPage);         // 4 byte
            span.Write(entry.Length);           // 4 byte
            span.Write(entry.CompressedLength); // 4 byte

            span.Write((int) entry.Modified.Subtract(DT_FROM).TotalSeconds); // 4 byte
            span.WritePrefixedString(entry.Name, ReadStringPrefix.Short);    // 2 byte length + 'length' bytes
            span.Write((byte) entry.Flags);                                  // 1 byte
        }

        return handler.Pack(buff).ToArray();
    }
}