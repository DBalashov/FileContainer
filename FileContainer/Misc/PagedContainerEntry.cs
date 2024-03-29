using System;
using System.Diagnostics.CodeAnalysis;

namespace FileContainer;

public sealed class PagedContainerEntry
{
    public string Name { get; }

    /// <summary> in bytes </summary>
    public int Length { get; private set; }

    /// <summary> in bytes </summary>
    public int CompressedLength { get; private set; }

    public EntryFlags Flags { get; private set; }

    /// <summary> UTC </summary>
    public DateTime Modified { get; private set; }

    public int FirstPage { get; private set; }
    public int LastPage  { get; private set; }

    internal PagedContainerEntry(string name, int pageFirst, int pageLast, int rawLength, int compressedLength, EntryFlags flags, DateTime modified)
    {
        Name = name;
        Update(pageFirst, pageLast, rawLength, compressedLength, flags);
        Modified = modified;
    }

    public void Update(int pageFirst, int pageLast, int rawLength, int compressedLength, EntryFlags flags)
    {
        FirstPage        = pageFirst;
        LastPage         = pageLast;
        Length           = rawLength;
        CompressedLength = compressedLength;
        Modified         = DateTime.UtcNow;
        Flags            = flags;
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Name}: {Length} bytes ({Modified:u}), FP: {FirstPage}";
}

[Flags]
public enum EntryFlags
{
    Compressed = 1
}