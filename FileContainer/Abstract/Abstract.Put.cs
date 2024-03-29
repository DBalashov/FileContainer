using System;
using System.Collections.Generic;
using System.Linq;

namespace FileContainer;

public abstract partial class PagedContainerAbstract
{
    /// <summary> Create or replace entry with specified key. </summary>
    /// <exception cref="ArgumentException"></exception>
    public virtual PutAppendResult Put(string key, byte[] data)
    {
        if (string.IsNullOrEmpty(key))
            throw new ArgumentNullException(nameof(key));

        if (data == null || data.Length == 0)
            throw new ArgumentNullException(nameof(data), "Argument can't be null or empty");

        throwIfHasOpenedStream(new[] {key});
        if (key.ContainMask())
            throw new ArgumentException($"Invalid name: {key}");

        var r = put(key, data);

        if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
            WriteHeaders();

        return r;
    }

    /// <summary>
    /// Create or replace of passed entries.
    /// Value in dictionary must not be null or empty.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public virtual Dictionary<string, PutAppendResult> Put(Dictionary<string, byte[]> keyValues)
    {
        throwIfHasOpenedStream(keyValues.Keys);
        foreach (var item in keyValues)
        {
            if (string.IsNullOrEmpty(item.Key))
                throw new ArgumentNullException(nameof(keyValues), "Argument can't be null or empty");

            if (item.Value == null || item.Value.Length == 0)
                throw new ArgumentNullException(nameof(keyValues), $"Argument can't be null or empty: {item.Key}");

            if (item.Key.ContainMask())
                throw new ArgumentException($"Invalid name: {item.Key}");
        }

        var r = new Dictionary<string, PutAppendResult>();
        foreach (var item in keyValues)
            r.Add(item.Key, put(item.Key, item.Value));

        if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
            WriteHeaders();

        return r;
    }

    PutAppendResult put(string key, byte[] data)
    {
        var rawLength = data.Length;
        data = Header.DataHandler.Pack(data).ToArray();

        var requiredPages = Header.GetRequiredPages(data.Length);
        var entryFlags    = Header.CompressType != PersistentContainerCompressType.None ? EntryFlags.Compressed : 0;

        if (entries.TryGet(key, out var existingEntry) && existingEntry != null)
        {
            var allocatedPages = Stream.ReadPageSequence(Header, existingEntry.FirstPage);
            if (requiredPages > allocatedPages.Length) // new data require additional pages?
            {
                // allocate require page count
                var newAllocatedPages = pageAllocator.AllocatePages(requiredPages - allocatedPages.Length);
                var newPages          = allocatedPages.Concat(newAllocatedPages).ToArray(); // append pages to current list
                allocatedPages = newPages;
            }
            else if (requiredPages < allocatedPages.Length) // new data length < than exists?
            {
                var mustBeFreePages = allocatedPages.Skip(requiredPages).ToArray(); // skip occupated pages 
                var newPages        = allocatedPages.Take(requiredPages).ToArray();

                pageAllocator.FreePages(mustBeFreePages);
                allocatedPages = newPages;
            } // else no action - new data will written into existing pages 

            Stream.WriteIntoPages(Header, data, 0, allocatedPages);
            entries.Update(existingEntry,
                           allocatedPages.First(), allocatedPages.Last(),
                           rawLength, data.Length,
                           entryFlags);
            return PutAppendResult.Updated;
        }

        var pages = pageAllocator.AllocatePages(requiredPages);
        Stream.WriteIntoPages(Header, data, 0, pages);
        entries.Add(new PagedContainerEntry(key,
                                            pages.First(), pages.Last(),
                                            rawLength, data.Length,
                                            entryFlags, DateTime.UtcNow));
        return PutAppendResult.Created;
    }
}

public enum PutAppendResult
{
    Created,
    Updated
}