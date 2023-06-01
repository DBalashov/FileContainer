using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileContainer;

static class ReadExtenders
{
    /// <summary>
    /// Read data and page numbers, starting from 'startFromPage'
    /// Used in PageAllocator & PagedContainerEntryCollection
    /// </summary>
    internal static PageSequence ReadWithPageSequence(this Stream stm, PagedContainerHeader header, int startFromPage)
    {
        var listOfPages = new List<Memory<byte>>();
        var pages       = new List<int>();

        while (startFromPage > 0)
        {
            pages.Add(startFromPage);

            stm.Position = startFromPage * header.PageSize;
            var buff = new byte[header.PageSize]; // todo rent
            var read = stm.Read(buff, 0, header.PageSize);
            if (read < header.PageSize)
                throw new Exception("ReadWithPageSequence: Read less than expected, data corrupted");

            listOfPages.Add(buff.AsMemory(0, header.PageUserDataSize));
            startFromPage = BitConverter.ToInt32(buff, header.PageUserDataSize);
        }

        var entirePages = mergeToArray(listOfPages);
        return new PageSequence(entirePages, pages.ToArray());
    }

    /// <summary> Read data from pages, starting with entry.FirstPage </summary>
    internal static byte[] ReadEntryPageSequence(this Stream stm, PagedContainerHeader header, PagedContainerEntry entry)
    {
        var listOfPages = new List<Memory<byte>>();

        var remainLength     = entry.Flags.HasFlag(EntryFlags.Compressed) ? entry.CompressedLength : entry.Length;
        var currentPageIndex = entry.FirstPage;

        while (currentPageIndex > 0)
        {
            stm.Position = currentPageIndex * header.PageSize;

            var buff = new byte[header.PageSize]; // todo rent
            var read = stm.Read(buff, 0, header.PageSize);
            if (read < header.PageSize)
                throw new Exception("ReadEntryPageSequence: Read less than expected, data corrupted");

            var requestLength = remainLength > header.PageUserDataSize ? header.PageUserDataSize : remainLength;

            listOfPages.Add(buff.AsMemory(0, requestLength));
            remainLength -= requestLength;

            currentPageIndex = BitConverter.ToInt32(buff, header.PageUserDataSize);
        }

        var entirePages = mergeToArray(listOfPages);
        return header.DataHandler.Unpack(entirePages).ToArray();
    }

    /// <summary>
    /// Read only sequence page numbers from 'startFromPage'.
    /// Last 32 bit contain next page index.
    /// Last page in sequence contain 'next page index' value == 0
    /// </summary>
    internal static int[] ReadPageSequence(this Stream stm, PagedContainerHeader header, int startFromPage)
    {
        var pages            = new List<int>();
        var buff             = new byte[4];
        var currentPageIndex = startFromPage;
        while (currentPageIndex > 0)
        {
            pages.Add(currentPageIndex);

            stm.Position = currentPageIndex * header.PageSize + header.PageUserDataSize;
            var read = stm.Read(buff, 0, buff.Length);
            if (read < buff.Length) break;

            currentPageIndex = BitConverter.ToInt32(buff, 0);
        }

        return pages.ToArray();
    }

    static byte[] mergeToArray(List<Memory<byte>> segments)
    {
        var accumulator = new byte[segments.Sum(p => p.Length)];
        var offset      = 0;

        foreach (var page in segments)
        {
            page.CopyTo(accumulator.AsMemory(offset, page.Length));
            offset += page.Length;
        }

        return accumulator;
    }
}