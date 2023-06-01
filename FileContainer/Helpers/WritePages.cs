using System;
using System.IO;

namespace FileContainer;

static class WriteExtenders
{
    /// <summary>
    /// Write buffer to targetPages.
    /// Last of 32 bit of each page contain next page index.
    /// Last page in sequence contain next page index == 0
    /// </summary>
    internal static void WriteIntoPages(this Stream stm, PagedContainerHeader header, Span<byte> data, int offset, int[] targetPages)
    {
        var currentPageIndex = 0;

        var page = new byte[header.PageSize].AsSpan();
        foreach (var pageIndex in targetPages)
        {
            var writeLength = data.Length - offset;
            if (writeLength > header.PageUserDataSize)
            {
                data.Slice(offset, header.PageUserDataSize).CopyTo(page);
                offset += header.PageUserDataSize;
            }
            else
            {
                page.Clear();
                data.Slice(offset, writeLength).CopyTo(page);
                offset += writeLength;
            }

            BitConverter.TryWriteBytes(page.Slice(header.PageUserDataSize),
                                       currentPageIndex + 1 < targetPages.Length ? targetPages[currentPageIndex + 1] : 0);

            var newPosition = header.PageSize * pageIndex;
            if (newPosition != stm.Position)
                stm.Position = newPosition;
            stm.Write(page);

            currentPageIndex++;
        }
    }
}