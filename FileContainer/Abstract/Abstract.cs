using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract : IDisposable
    {
        [NotNull] internal readonly Stream                        stm;
        [NotNull] internal readonly PagedContainerHeader          header;
        [NotNull] readonly          PageAllocator                 pageAllocator;
        [NotNull] readonly          PagedContainerEntryCollection entries;

        public int   PageSize   => header.PageSize;
        public int   TotalPages => pageAllocator.TotalPages;
        public int   FreePages  => pageAllocator.GetFreePages().Count();
        public Int64 Length     => stm.Length;

        #region constructor / dispose

        protected PagedContainerAbstract([NotNull] Stream stm, int pageSize = 4096)
        {
            this.stm = stm;

            try
            {
                if (stm.Length == 0) // new file
                {
                    header = new PagedContainerHeader(pageSize);
                    header.Write(stm);

                    pageAllocator = new PageAllocator(header);
                    pageAllocator.Write(stm);
                }
                else
                {
                    header        = new PagedContainerHeader(stm);
                    pageAllocator = new PageAllocator(stm, header);
                }

                entries = header.DirectoryFirstPage == 0
                    ? new PagedContainerEntryCollection()
                    : new PagedContainerEntryCollection(stm.ReadWithPageSequence(header, header.DirectoryFirstPage));
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

            if (stm.CanWrite && entries.Modified)
            {
                entries.Write(stm, header, pageAllocator);
                stm.Flush();
            }

            DisposeStream();
        }

        protected virtual void DisposeStream()
        {
            stm.Close();
            stm.Dispose();
        }

        #endregion

        /// <summary> Find entries by names. Mask chars * and ? supported in keys </summary>
        [NotNull]
        public PagedContainerEntry[] Find(params string[] keys) =>
            (keys.Any()
                ? entries.Find(keys)
                : entries.All()).ToArray();

        /// <summary> Remove entries by keys. Mask * and ? supported. Return deleted keys. </summary>
        [NotNull]
        public virtual string[] Delete(params string[] keys)
        {
            throwIfHasOpenedStream(keys);

            var r = new List<string>();
            foreach (var entry in entries.Find(keys).ToArray())
            {
                r.Add(entry.Name);
                var pages = stm.ReadPageSequence(header, entry.FirstPage);
                pageAllocator.FreePages(pages);
                entries.Remove(entry);
            }

            return r.ToArray();
        }

        public byte[] this[string key]
        {
            [CanBeNull] get => Get(key);
            [NotNull] set => Put(key, value);
        }
    }
}