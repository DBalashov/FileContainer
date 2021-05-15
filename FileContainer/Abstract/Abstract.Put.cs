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
        static readonly Encoding defaultEncoding = Encoding.UTF8;
        
        #region byte[]

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

        #endregion
        
        #region strings

        /// <summary> Create or replace entry with specified key </summary>
        /// <exception cref="ArgumentException"></exception>
        public virtual PutAppendResult Put([NotNull] string key, [NotNull] string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Argument can't be null or empty", nameof(data));

            return Put(key, defaultEncoding.GetBytes(data));
        }

        /// <summary>
        /// Create or replace of passed entries.
        /// Value in dictionary must not be null or empty.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        [NotNull]
        public virtual Dictionary<string, PutAppendResult> Put([NotNull] Dictionary<string, string> keyValues)
        {
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (string.IsNullOrEmpty(item.Value))
                    throw new ArgumentException($"Argument can't be null or empty: {item.Key}", nameof(keyValues));
            }

            return Put(keyValues.ToDictionary(p => p.Key, p => defaultEncoding.GetBytes(p.Value), StringComparer.InvariantCultureIgnoreCase));
        }
        
        #endregion
        
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

            var pages = pageAllocator.AllocatePages(requiredPages);
            stm.WriteIntoPages(header, data, 0, pages);
            entries.Add(new PagedContainerEntry(key, pages.First(), pages.Last(), data.Length, 0, DateTime.UtcNow));
            return PutAppendResult.Created;
        }

    }

    public enum PutAppendResult
    {
        Created,
        Updated
    }
}