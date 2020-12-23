using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    class PagedContainerEntryCollection
    {
        [NotNull] readonly Dictionary<string, PagedContainerEntry> entries;
        [NotNull]          int[]                                   pages;

        internal PagedContainerEntryCollection()
        {
            entries = new Dictionary<string, PagedContainerEntry>();
            pages   = new int[0];
        }

        internal PagedContainerEntryCollection(PageSequence ps)
        {
            entries = PagedContainerEntry.Unpack(ps.Data).ToDictionary(p => p.Name, StringComparer.InvariantCultureIgnoreCase);
            pages   = ps.Pages;
        }

        internal bool TryGet([NotNull] string key, out PagedContainerEntry item) =>
            entries.TryGetValue(key, out item);

        internal void Add([NotNull] PagedContainerEntry entry) =>
            entries.Add(entry.Name, entry);

        internal bool Remove([NotNull] PagedContainerEntry entry)
        {
            if (!entries.ContainsKey(entry.Name)) return false;

            entries.Remove(entry.Name);
            return true;
        }

        [NotNull]
        internal IEnumerable<PagedContainerEntry> Find(params string[] keys)
        {
            var processed = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
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

        [NotNull]
        internal IEnumerable<PagedContainerEntry> All() => entries.Values.ToArray();

        /// <summary>
        /// Write entries directory to pages. If pages not enough for new directory - additional pages will allocated.
        /// Also write header and pageAllocator state
        /// </summary>
        internal void Write([NotNull] Stream stm, [NotNull] PagedContainerHeader header, [NotNull] PageAllocator pageAllocator)
        {
            var targetPages = pages;

            var buff          = PagedContainerEntry.Pack(entries.Values.ToArray());
            var requiredPages = header.GetRequiredPages(buff.Length);

            if (requiredPages > targetPages.Length) // новые данные занимают больше страниц?
            {
                // довыделяем требуемое количество страниц
                targetPages = targetPages.Concat(pageAllocator.AllocatePages(requiredPages - targetPages.Length)).ToArray();
            }
            else if (requiredPages < targetPages.Length) // новые данные занимают меньше страниц?
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
    }
}