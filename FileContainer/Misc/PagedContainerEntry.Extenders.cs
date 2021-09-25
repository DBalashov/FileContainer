using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

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

            buff = handler.Unpack(buff).ToArray();

            var offset = 0;
            var count  = buff.GetInt(ref offset);
            for (var i = 0; i < count; i++)
            {
                var firstPage        = buff.GetInt(ref offset); // 4 byte
                var lastPage         = buff.GetInt(ref offset); // 4 byte
                var rawLength        = buff.GetInt(ref offset); // 4 byte
                var compressedLength = buff.GetInt(ref offset); // 4 byte
                if (compressedLength == 0)
                    rawLength = compressedLength;

                var modified = buff.GetInt(ref offset);    // 4 byte
                var name     = buff.GetString(ref offset); // 2 byte length + 'length' bytes 
                var flags    = (EntryFlags)buff[offset++]; // 1 byte

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