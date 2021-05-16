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
        [NotNull] public string Name { get; }

        /// <summary> in bytes </summary>
        public int Length { get; internal set; }

        /// <summary> in bytes </summary>
        public int CompressedLength { get; internal set; }

        public EntryFlags Flags { get; internal set; }

        /// <summary> UTC </summary>
        public DateTime Modified { get; internal set; }

        public int FirstPage { get; internal set; }
        public int LastPage  { get; internal set; }

        internal PagedContainerEntry([NotNull] string name, int firstPage, int lastPage, int rawLength, int compressedLength, EntryFlags flags, DateTime modified)
        {
            Name             = name;
            FirstPage        = firstPage;
            LastPage         = lastPage;
            Length           = rawLength;
            CompressedLength = compressedLength;
            Modified         = modified;
            Flags            = flags;
        }

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Name}: {Length} bytes ({Modified:u}), FP: {FirstPage}";
    }

    [Flags]
    public enum EntryFlags
    {
    }
}