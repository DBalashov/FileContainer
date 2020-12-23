using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace FileContainer
{
    public class PagedContainerEntry
    {
        static readonly DateTime DT_FROM = new DateTime(2010, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        [NotNull] public string Name { get; }

        /// <summary> в байтах </summary>
        public int Length { get; internal set; }

        public KVEntryFlags Flags { get; internal set; }

        /// <summary> UTC </summary>
        public DateTime Modified { get; internal set; }

        public int FirstPage { get; internal set; }
        public int LastPage  { get; internal set; }

        internal PagedContainerEntry([NotNull] string name, int firstPage, int lastPage, int length, KVEntryFlags flags, DateTime modified)
        {
            Name      = name;
            FirstPage = firstPage;
            LastPage  = lastPage;
            Length    = length;
            Modified  = modified;
            Flags     = flags;
        }

        [NotNull]
        internal static IEnumerable<PagedContainerEntry> Unpack([NotNull] byte[] buff)
        {
            if (buff.Length <= 0) yield break;

            var offset = 0;
            var count  = buff.GetInt(ref offset);
            for (var i = 0; i < count; i++)
            {
                var firstPage = buff.GetInt(ref offset); // 4 byte
                var lastPage  = buff.GetInt(ref offset); // 4 byte

                var length   = buff.GetInt(ref offset); // 4 byte
                var reserved = buff.GetInt(ref offset); // 4 byte

                var modified = buff.GetInt(ref offset);       // 4 byte
                var name     = buff.GetString(ref offset);    // 2 byte length + 'length' bytes 
                var flags    = (KVEntryFlags) buff[offset++]; // 1 byte

                yield return new PagedContainerEntry(name, firstPage, lastPage, length, flags, DT_FROM.AddSeconds(modified));
            }
        }

        [NotNull]
        internal static byte[] Pack([NotNull] ICollection<PagedContainerEntry> entries)
        {
            using var stm = new MemoryStream();
            using var bw  = new BinaryWriter(stm, Encoding.Default, true);

            bw.Write(entries.Count);
            foreach (var entry in entries)
            {
                bw.Write(entry.FirstPage); // 4 byte
                bw.Write(entry.LastPage);  // 4 byte

                bw.Write(entry.Length); // 4 byte
                bw.Write((int) 0);      // 4 byte

                bw.Write((int) entry.Modified.Subtract(DT_FROM).TotalSeconds); // 4 byte
                bw.PutString(entry.Name);                                      // 2 byte length + 'length' bytes
                bw.Write((byte) entry.Flags);                                  // 1 byte
            }

            bw.Flush();
            return stm.ToArray();
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Name}: {Length} bytes ({Modified:u}), FP: {FirstPage}";
#endif
    }

    [Flags]
    public enum KVEntryFlags
    {
    }
}