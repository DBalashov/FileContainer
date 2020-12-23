using System;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    static class WriteExtenders
    {
        /// <summary>
        /// Write buffer to passed page numbers.
        /// Last of 32 bit of each page contain pointer for next page index.
        /// Last page in sequence contain point == 0
        /// </summary>
        internal static void WriteIntoPages([NotNull] this Stream stm, [NotNull] PagedContainerHeader header, [NotNull] byte[] data, int offset, [NotNull] int[] targetPages)
        {
            for (var i = 0; i < targetPages.Length; i++)
            {
                var pageIndex = targetPages[i];
                stm.Position = header.PageSize * pageIndex;

                stm.Write(data, offset, Math.Min(header.PageUserDataSize, data.Length - offset));
                offset += header.PageUserDataSize;

                var pageNextIndex = i + 1 < targetPages.Length ? targetPages[i + 1] : 0;

                var newPosition = header.PageSize * pageIndex + header.PageUserDataSize;
                if (newPosition != stm.Position)
                    stm.Position = newPosition;
                stm.Write(BitConverter.GetBytes(pageNextIndex), 0, 4);
            }
        }
    }
}