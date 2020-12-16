using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <summary> Create or replace entry with specified key </summary>
        public virtual void Put([NotNull] string key, [NotNull] byte[] data)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("Argument can't be null or empty", nameof(key));

            if (data == null || data.Length == 0)
                throw new ArgumentException("Argument can't be null or empty", nameof(data));
            
            throwIfHasOpenedStream(new[] {key});
            if (key.ContainMask())
                throw new ArgumentException($"Invalid name: {key}");

            put(key, data);
            entries.Write(stm, header, pageAllocator);
        }

        /// <summary> Create or replace of passed entries. Value in dictionary can't be null or empty </summary>
        public virtual void Put([NotNull] Dictionary<string, byte[]> keyValues)
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

            foreach (var item in keyValues)
                put(item.Key, item.Value);

            entries.Write(stm, header, pageAllocator);
        }

        void put([NotNull] string key, [NotNull] byte[] data)
        {
            var requiredPages = header.GetRequiredPages(data.Length);

            if (entries.TryGet(key, out var existingEntry))
            {
                var allocatedPages = stm.ReadPageSequence(header, existingEntry.FirstPage);

                if (allocatedPages.Length < requiredPages) // new data require additional pages?
                {
                    // allocate require page count
                    var newPages = pageAllocator.AllocatePages(requiredPages - allocatedPages.Length);
                    allocatedPages = allocatedPages.Concat(newPages).ToArray(); // append pages to current list
                }
                else if (requiredPages < allocatedPages.Length) // new data less than exists?
                {
                    var mustBeFreePages = allocatedPages.Skip(requiredPages).ToArray(); // skip occupated pages 
                    allocatedPages = allocatedPages.Take(requiredPages).ToArray();

                    pageAllocator.FreePages(mustBeFreePages);
                }

                stm.WriteIntoPages(header, data, 0, allocatedPages);
                existingEntry.LastPage = allocatedPages.Last();
                existingEntry.Modified = DateTime.UtcNow;
                existingEntry.Length   = data.Length;
            }
            else
            {
                var pages = pageAllocator.AllocatePages(requiredPages);
                stm.WriteIntoPages(header, data, 0, pages);
                entries.Add(new FileContainerEntry(key, pages.First(), pages.Last(), data.Length, 0, DateTime.UtcNow));
            }
        }
    }
}