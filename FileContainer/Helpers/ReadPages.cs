using System;
using System.Buffers;
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
        var listOfPages = new List<byte[]>();
        var pages       = new List<int>();

        var pool = ArrayPool<byte>.Shared;
        
        try
        {
            while (startFromPage > 0)
            {
                pages.Add(startFromPage);

                stm.Position = (long)startFromPage * header.PageSize;
                var buff = pool.Rent(header.PageSize);
                var read = stm.Read(buff, 0, header.PageSize);
                if (read < header.PageSize)
                    throw new Exception("ReadWithPageSequence: Read less than expected, data corrupted");

                var data = new byte[header.PageUserDataSize];
                Array.Copy(buff, 0, data, 0, header.PageUserDataSize);
                listOfPages.Add(data);
                
                startFromPage = BitConverter.ToInt32(buff, header.PageUserDataSize);
                pool.Return(buff);
            }
        }
        catch
        {
            // In case of error, we don't have an easy way to return already rented buffers if we didn't store them carefully.
            // But here we return them immediately after use except for the case where we copy data out.
            throw;
        }

        var entirePages = mergeToArray(listOfPages);
        return new PageSequence(entirePages, pages.ToArray());
    }

    /// <summary> Read data from pages, starting with entry.FirstPage </summary>
    internal static byte[] ReadEntryPageSequence(this Stream stm, PagedContainerHeader header, PagedContainerEntry entry)
    {
        var remainLength     = entry.Flags.HasFlag(EntryFlags.Compressed) ? entry.CompressedLength : entry.Length;
        var currentPageIndex = entry.FirstPage;

        var pool = ArrayPool<byte>.Shared;
        var buff = pool.Rent(header.PageSize);
        
        using var resultStream = MemoryStreamManager.Instance.GetStream();
        try
        {
            while (currentPageIndex > 0)
            {
                stm.Position = (long)currentPageIndex * header.PageSize;

                var read = stm.Read(buff, 0, header.PageSize);
                if (read < header.PageSize)
                    throw new Exception("ReadEntryPageSequence: Read less than expected, data corrupted");

                var requestLength = remainLength > header.PageUserDataSize ? header.PageUserDataSize : remainLength;

                resultStream.Write(buff, 0, requestLength);
                remainLength -= requestLength;

                currentPageIndex = BitConverter.ToInt32(buff, header.PageUserDataSize);
            }
        }
        finally
        {
            pool.Return(buff);
        }

        var entirePages = resultStream.ToArray();
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
        var buff             = (Span<byte>)stackalloc byte[4];
        var currentPageIndex = startFromPage;
        while (currentPageIndex > 0)
        {
            pages.Add(currentPageIndex);

            stm.Position = (long)currentPageIndex * header.PageSize + header.PageUserDataSize;
            var read = stm.Read(buff);
            if (read < buff.Length) break;

            currentPageIndex = BitConverter.ToInt32(buff);
        }

        return pages.ToArray();
    }

    static byte[] mergeToArray(List<byte[]> segments)
    {
        var accumulator = new byte[segments.Sum(p => p.Length)];
        var offset      = 0;

        foreach (var page in segments)
        {
            Buffer.BlockCopy(page, 0, accumulator, offset, page.Length);
            offset += page.Length;
        }

        return accumulator;
    }
}