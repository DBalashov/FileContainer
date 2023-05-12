using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SpanByteExtenders;

namespace FileContainer
{
    static class PagedContainerEntryExtenders
    {
        static readonly DateTime DT_FROM = new(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        internal static Dictionary<string, PagedContainerEntry> ReadEntries(this byte[] buff, IDataHandler handler)
        {
            var r = new Dictionary<string, PagedContainerEntry>(StringComparer.InvariantCultureIgnoreCase);

            if (buff.Length == 0)
                return r;

            var span = handler.Unpack(buff);

            var count = span.ReadInt32();
            for (var i = 0; i < count; i++)
            {
                var firstPage        = span.ReadInt32();
                var lastPage         = span.ReadInt32();
                var rawLength        = span.ReadInt32();
                var compressedLength = span.ReadInt32();
                if (compressedLength == 0)
                    rawLength = compressedLength;

                var modified = span.ReadInt32();
                var name     = span.ReadPrefixedString(ReadStringPrefix.Short);
                var flags    = (EntryFlags) span.ReadByte();

                r.Add(name, new PagedContainerEntry(name, firstPage, lastPage, rawLength, compressedLength, flags, DT_FROM.AddSeconds(modified)));
            }

            return r;
        }
        
        internal static byte[] WriteEntries(this Dictionary<string, PagedContainerEntry> entries, IDataHandler handler)
        {
            using var stm = new MemoryStream();
            using var bw  = new BinaryWriter(stm, Encoding.Default, true);

            bw.Write(entries.Count);
            foreach (var kvp in entries)
            {
                var entry = kvp.Value;

                bw.Write(entry.FirstPage);        // 4 byte
                bw.Write(entry.LastPage);         // 4 byte
                bw.Write(entry.Length);           // 4 byte
                bw.Write(entry.CompressedLength); // 4 byte

                bw.Write((int)entry.Modified.Subtract(DT_FROM).TotalSeconds); // 4 byte
                bw.PutString(entry.Name);                                     // 2 byte length + 'length' bytes
                bw.Write((byte)entry.Flags);                                  // 1 byte
            }

            bw.Flush();
            return handler.Pack(stm.ToArray()).ToArray();
        }
    }
}