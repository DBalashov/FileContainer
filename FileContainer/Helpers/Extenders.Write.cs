using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    static class WriteExtenders
    {
        /// <summary>
        /// Write buffer to targetPages.
        /// Last of 32 bit of each page contain next page index.
        /// Last page in sequence contain next page index == 0
        /// </summary>
        internal static void WriteIntoPages([NotNull] this Stream stm, [NotNull] PagedContainerHeader header, [NotNull] byte[] data, int offset, [NotNull] int[] targetPages)
        {
            var currentPageIndex = 0;

            var page = new byte[header.PageSize];
            foreach (var pageIndex in targetPages)
            {
                var writeLength = data.Length - offset;
                if (writeLength > header.PageUserDataSize)
                {
                    Array.Copy(data, offset, page, 0, header.PageUserDataSize);
                    offset += header.PageUserDataSize;
                }
                else
                {
                    Array.Clear(page, 0, page.Length);
                    Array.Copy(data, offset, page, 0, writeLength);
                    offset += writeLength;
                }

                Array.Copy(BitConverter.GetBytes(currentPageIndex + 1 < targetPages.Length ? targetPages[currentPageIndex + 1] : 0), 0,
                           page, header.PageUserDataSize, 4);

                var newPosition = header.PageSize * pageIndex;
                if (newPosition != stm.Position)
                    stm.Position = newPosition;
                stm.Write(page, 0, header.PageSize);

                currentPageIndex++;
            }
        }
    }
}