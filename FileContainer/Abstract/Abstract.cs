using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract : IDisposable
    {
        [NotNull] internal readonly Stream                       stm;
        [NotNull] internal readonly FileContainerHeader          header;
        [NotNull] readonly          PageAllocator                pageAllocator;
        [NotNull] readonly          FileContainerEntryCollection entries;

        public int PageSize => header.PageSize;

        #region constructor / dispose

        protected PagedContainerAbstract([NotNull] Stream stm, int pageSize = 4096)
        {
            this.stm = stm;

            try
            {
                if (stm.Length == 0) // new file
                {
                    header = new FileContainerHeader(pageSize);
                    header.Write(stm);

                    pageAllocator = new PageAllocator(header);
                    pageAllocator.Write(stm);
                }
                else
                {
                    header        = new FileContainerHeader(stm);
                    pageAllocator = new PageAllocator(stm, header);
                }

                entries = header.DirectoryFirstPage == 0
                    ? new FileContainerEntryCollection()
                    : new FileContainerEntryCollection(stm.ReadWithPageSequence(header, header.DirectoryFirstPage));
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
            if (stm.CanWrite)
                stm.Flush();

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
        public FileContainerEntry[] Find(params string[] keys) =>
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

            entries.Write(stm, header, pageAllocator);
            return r.ToArray();
        }

        public byte[] this[string key]
        {
            [CanBeNull] get => Get(key);
            [NotNull] set => Put(key, value);
        }
    }
}