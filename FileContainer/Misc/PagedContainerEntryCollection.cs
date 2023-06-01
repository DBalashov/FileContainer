using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FileContainer;

sealed class PagedContainerEntryCollection
{
    readonly Dictionary<string, PagedContainerEntry> entries;
    int[]                                            pages;

    public bool Modified { get; private set; }

    internal PagedContainerEntryCollection()
    {
        entries = new Dictionary<string, PagedContainerEntry>();
        pages   = Array.Empty<int>();
    }

    #region Read / Write

    internal PagedContainerEntryCollection(PagedContainerHeader header, PageSequence ps)
    {
        entries  = ps.Data.ReadEntries(header.DataHandler);
        pages    = ps.Pages;
        Modified = false;
    }

    /// <summary>
    /// Write entries directory to pages. If pages not enough for new directory - additional pages will allocated.
    /// Also write header and pageAllocator state
    /// </summary>
    public void Write(Stream stm, PagedContainerHeader header, PageAllocator pageAllocator)
    {
        var targetPages = pages;

        var buff = entries.WriteEntries(header.DataHandler);

        var requiredPages = header.GetRequiredPages(buff.Length);

        if (requiredPages > targetPages.Length) // need to allocate additional pages?
        {
            targetPages = targetPages.Concat(pageAllocator.AllocatePages(requiredPages - targetPages.Length)).ToArray();
        }
        else if (requiredPages < targetPages.Length) // can free unused pages?
        {
            var mustBeFreePages = targetPages.Skip(requiredPages).ToArray();
            targetPages = targetPages.Take(requiredPages).ToArray();

            pageAllocator.FreePages(mustBeFreePages);
        }

        stm.WriteIntoPages(header, buff, 0, targetPages);

        header.DirectoryFirstPage = targetPages[0];
        header.Write(stm);

        pageAllocator.Write(stm);

        pages = targetPages;
    }

    #endregion

    #region TryGet / Add / Remove / Update

    public bool TryGet(string key, out PagedContainerEntry? item) =>
        string.IsNullOrEmpty(key)
            ? throw new ArgumentException("Argument can't be null or empty", nameof(key))
            : entries.TryGetValue(key, out item);

    public void Add(PagedContainerEntry entry)
    {
        entries.Add(entry.Name, entry);
        Modified = true;
    }

    public bool Remove(PagedContainerEntry entry)
    {
        if (!entries.ContainsKey(entry.Name)) return false;

        entries.Remove(entry.Name);
        Modified = true;

        return true;
    }

    public void Update(PagedContainerEntry entry, int pageFirst, int pageLast, int rawLength, int compressedLength, EntryFlags flags)
    {
        entry.Update(pageFirst, pageLast, rawLength, compressedLength, flags);
        Modified = true;
    }

    #endregion

    /// <exception cref="ArgumentException"></exception>
    public IEnumerable<PagedContainerEntry> Find(params string[] keys)
    {
        var processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        foreach (var key in keys)
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Argument can't be null or empty", nameof(keys));

        foreach (var key in keys)
        {
            if (key.ContainMask())
            {
                foreach (var entry in entries)
                    if (!processed.Contains(entry.Key) && PatternMatcher.Match(entry.Key, key))
                    {
                        processed.Add(entry.Key);
                        yield return entry.Value;
                    }
            }
            else
            {
                if (entries.TryGetValue(key, out var entry) && !processed.Contains(key))
                {
                    processed.Add(key);
                    yield return entry;
                }
            }
        }
    }


    public IEnumerable<PagedContainerEntry> All() => entries.Values.ToArray();

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"Pages: {pages.Length}, Entries: {entries.Count}";
}