using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <summary> Create or replace entry with specified key </summary>
        public virtual PutAppendResult Put([NotNull] string key, [NotNull] byte[] data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Argument can't be null or empty", nameof(key));

            if (data == null || data.Length == 0)
                throw new ArgumentException("Argument can't be null or empty", nameof(data));

            throwIfHasOpenedStream(new[] {key});
            if (key.ContainMask())
                throw new ArgumentException($"Invalid name: {key}");

            var r = put(key, data);
            return r;
        }

        /// <summary> Create or replace of passed entries. Value in dictionary can't be null or empty </summary>
        [NotNull]
        public virtual Dictionary<string, PutAppendResult> Put([NotNull] Dictionary<string, byte[]> keyValues)
        {
            throwIfHasOpenedStream(keyValues.Keys);
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (item.Value == null || item.Value.Length == 0)
                    throw new ArgumentException($"Argument can't be null or empty {item.Key}", nameof(keyValues));

                if (item.Key.ContainMask())
                    throw new ArgumentException($"Invalid name: {item.Key}");
            }

            var r = new Dictionary<string, PutAppendResult>();
            foreach (var item in keyValues)
                r.Add(item.Key, put(item.Key, item.Value));
            
            return r;
        }

        PutAppendResult put([NotNull] string key, [NotNull] byte[] data)
        {
            var requiredPages = header.GetRequiredPages(data.Length);

            if (entries.TryGet(key, out var existingEntry))
            {
                var allocatedPages = stm.ReadPageSequence(header, existingEntry.FirstPage);
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
                }

                stm.WriteIntoPages(header, data, 0, allocatedPages);
                entries.Update(existingEntry, allocatedPages.First(), allocatedPages.Last(), data.Length);
                return PutAppendResult.Updated;
            }
            else
            {
                var pages = pageAllocator.AllocatePages(requiredPages);
                stm.WriteIntoPages(header, data, 0, pages);
                entries.Add(new PagedContainerEntry(key, pages.First(), pages.Last(), data.Length, 0, DateTime.UtcNow));
                return PutAppendResult.Created;
            }
        }
    }

    public enum PutAppendResult
    {
        Created,
        Updated
    }
}