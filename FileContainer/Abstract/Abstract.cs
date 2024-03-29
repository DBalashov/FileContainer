using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FileContainer;

public abstract partial class PagedContainerAbstract : IDisposable
{
    public const int MinPageSize = PagedContainerHeader.MIN_PAGE_SIZE;
    public const int MaxPageSize = PagedContainerHeader.MAX_PAGE_SIZE;

    internal readonly Stream                        Stream;
    internal readonly PagedContainerHeader          Header;
    readonly          PageAllocator                 pageAllocator;
    readonly          PagedContainerEntryCollection entries;

    /// <summary> page size (bytes) </summary>
    public int PageSize => Header.PageSize;

    /// <summary> total pages in container file </summary>
    public int TotalPages => pageAllocator.TotalPages;

    /// <summary> free pages in container file </summary>
    public int FreePages => pageAllocator.GetFreePages().Count();

    /// <summary> length of container file (bytes) </summary>
    public Int64 Length => Stream.Length;

    public PersistentContainerFlags Flags => Header.Flags;

    #region constructor / dispose

    protected PagedContainerAbstract(Stream stm, PersistentContainerSettings? settings = null)
    {
        Stream   =   stm;
        settings ??= new PersistentContainerSettings();
        try
        {
            if (stm.Length == 0) // new file
            {
                Header = new PagedContainerHeader(settings);
                Header.Write(stm);

                pageAllocator = new PageAllocator(Header);
                pageAllocator.Write(stm);
            }
            else
            {
                Header        = new PagedContainerHeader(stm, settings.encryptorDecryptor);
                pageAllocator = new PageAllocator(Header, stm);
            }

            entries = Header.DirectoryFirstPage == 0
                          ? new PagedContainerEntryCollection()
                          : new PagedContainerEntryCollection(Header, stm.ReadWithPageSequence(Header, Header.DirectoryFirstPage));
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    bool isDisposed = false;

    public void Dispose()
    {
        if (isDisposed) return;
        isDisposed = true;

        if (!Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
            WriteHeaders();

        DisposeStream();
    }

    protected void WriteHeaders()
    {
        if (!Stream.CanWrite || (entries == null! || !entries.Modified)) return;

        entries.Write(Stream, Header, pageAllocator);
        Stream.Flush();
    }

    protected virtual void DisposeStream()
    {
        Stream.Close();
        Stream.Dispose();
    }

    #endregion

    /// <summary>
    /// Find entries by names.
    /// Mask chars * and ? supported in keys.
    /// Return ALL entries if no keys passed.
    /// </summary>
    public PagedContainerEntry[] Find(params string[] keys) =>
        (keys.Any()
             ? entries.Find(keys)
             : entries.All()).ToArray();

    /// <summary>
    /// Remove entries by keys. Mask * and ? supported.
    /// Return deleted keys.
    /// </summary>
    public virtual string[] Delete(params string[] keys)
    {
        throwIfHasOpenedStream(keys);

        var r = new List<string>();
        foreach (var entry in entries.Find(keys).ToArray())
        {
            r.Add(entry.Name);
            var pages = Stream.ReadPageSequence(Header, entry.FirstPage);
            pageAllocator.FreePages(pages);
            entries.Remove(entry);
        }

        if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
            WriteHeaders();

        return r.ToArray();
    }

    public byte[]? this[string key]
    {
        get => Get(key);
        set => Put(key, value!);
    }

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"{Header}, Entries: {entries}, Length={Length}";
}