using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;

namespace FileContainer
{
    /// <summary>
    /// Allocator and store for pages
    /// All pages stored as bit array with two state - free and occupated 
    /// First two pages always occupated - header and first page of PageAllocator
    ///
    /// If count of current pages not enough for AllocatePages - page array will expanded to required pages * PAGE_ALLOC_MULTIPLIER
    /// </summary>
    sealed class PageAllocator
    {
        const int PAGE_ALLOC_MULTIPLIER = 4;

        const int FIRST_DATA_PAGE = 2;
        const int FIRST_PA_PAGE   = 1;

        readonly ExpandableBitArray   pageAllocations;
        readonly PagedContainerHeader header;

        public int TotalPages => pageAllocations.Length;

        public PageAllocator(PagedContainerHeader header)
        {
            this.header = header;

            pageAllocations    = new ExpandableBitArray(2);
            pageAllocations[0] = true;
            pageAllocations[1] = true;
        }

        public PageAllocator(PagedContainerHeader header, Stream stm)
        {
            this.header = header;

            var buff = stm.ReadWithPageSequence(header, FIRST_PA_PAGE).Data;
            pageAllocations = new ExpandableBitArray(header.DataHandler.Unpack(buff).ToArray());
        }

        /// <summary> write self into stm with allocate pages with self </summary>
        public void Write(Stream stm)
        {
            var buff           = header.DataHandler.Pack(pageAllocations.GetBytes());
            var allocatedPages = stm.ReadPageSequence(header, FIRST_PA_PAGE).ToList();
            var requiredPages  = header.GetRequiredPages(buff.Length);
            while (requiredPages > allocatedPages.Count)
            {
                allocatedPages.AddRange(AllocatePages(requiredPages - allocatedPages.Count));
                buff          = header.DataHandler.Pack(pageAllocations.GetBytes()); // serialize to buffer and check again - need to more pages?
                requiredPages = header.GetRequiredPages(buff.Length);
            }

            stm.WriteIntoPages(header, buff, 0, allocatedPages.ToArray());
        }

        #region Allocate / Free

        public int[] AllocatePages(int count)
        {
            if (count <= 0)
                throw new ArgumentException($"Allocate: invalid page count: {count}");

            var r       = new int[count];
            var counter = 0;

            foreach (var pageIndex in pageAllocations.GetBits(false).Take(count))
                r[counter++] = pageIndex;

            var additionalPages = count - counter;
            if (additionalPages > 0) // запрашиваемых страниц больше, чем есть в pageAllocations -> увеличиваем его емкость на эту разницу * PAGE_ALLOC_MULTIPLIER
            {
                pageAllocations.ResizeTo(pageAllocations.Length + additionalPages * PAGE_ALLOC_MULTIPLIER);

                counter = 0;
                foreach (var pageIndex in pageAllocations.GetBits(false).Take(count))
                    r[counter++] = pageIndex;
            }

            foreach (var page in r)
            {
                if (pageAllocations[page])
                    throw new InvalidOperationException($"Free: Page already allocated: {page}");

                pageAllocations[page] = true;
            }

            return r;
        }

        public void FreePages(params int[] pages)
        {
            foreach (var page in pages)
            {
                if (page < FIRST_DATA_PAGE)
                    throw new InvalidOperationException($"Free: Page reserved and unavailable: {page}");

                if (!pageAllocations[page])
                    throw new InvalidOperationException($"Free: Page already free: {page}");

                pageAllocations[page] = false;
            }
        }

        #endregion

        [ExcludeFromCodeCoverage] // not used now
        public IEnumerable<int> GetFreePages()
        {
            for (var i = FIRST_DATA_PAGE; i < pageAllocations.Length; i++)
                if (!pageAllocations[i])
                    yield return i;
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"Pages: {pageAllocations.Length}";
#endif
    }
}