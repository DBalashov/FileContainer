using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <summary> Create or replace entry with specified key. </summary>
        /// <exception cref="ArgumentException"></exception>
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

            if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
                WriteHeaders();

            return r;
        }

        /// <summary>
        /// Create or replace of passed entries.
        /// Value in dictionary must not be null or empty.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        [NotNull]
        public virtual Dictionary<string, PutAppendResult> Put([NotNull] Dictionary<string, byte[]> keyValues)
        {
            throwIfHasOpenedStream(keyValues.Keys);
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (item.Value == null || item.Value.Length == 0)
                    throw new ArgumentException($"Argument can't be null or empty: {item.Key}", nameof(keyValues));

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

        PutAppendResult put([NotNull] string key, [NotNull] byte[] data)
        {
            var rawLength = data.Length;

            var requiredPages = Header.GetRequiredPages(data.Length);
            var entryFlags    = Header.Flags.HasFlag(PersistentContainerFlags.Compressed) ? EntryFlags.Compressed : 0;

            if (Header.Flags.HasFlag(PersistentContainerFlags.Compressed))
            {
                var compressedData              = Header.DataHandler.Pack(data);
                var compressedDataRequiredPages = Header.GetRequiredPages(compressedData.Length);
                if (compressedDataRequiredPages >= requiredPages) // compressed data >= original data?
                {
                    entryFlags = 0; // no compression, write raw
                }
                else
                {
                    data          = compressedData;
                    requiredPages = compressedDataRequiredPages;
                }
            }

            if (entries.TryGet(key, out var existingEntry))
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
}