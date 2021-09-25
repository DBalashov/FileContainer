using System;
using System.Collections.Generic;
using System.IO;

namespace FileContainer
{
    static class ReadExtenders
    {
        /// <summary>
        /// Read data and page numbers, starting from 'startFromPage'
        /// Used in PageAllocator & PagedContainerEntryCollection
        /// </summary>
        internal static PageSequence ReadWithPageSequence(this Stream stm, PagedContainerHeader header, int startFromPage)
        {
            using var stmCollector = new MemoryStream();

            var pages = new List<int>();
            var buff  = new byte[header.PageSize];

            while (startFromPage > 0)
            {
                pages.Add(startFromPage);
                stm.Position = startFromPage * header.PageSize;
                stm.Read(buff, 0, header.PageSize);

                stmCollector.Write(buff, 0, header.PageUserDataSize);
                startFromPage = BitConverter.ToInt32(buff, header.PageUserDataSize);
            }

            return new PageSequence(stmCollector.ToArray(), pages.ToArray());
        }

        /// <summary> Read data from pages, starting with entry.FirstPage </summary>
        internal static byte[] ReadEntryPageSequence(this Stream stm, PagedContainerHeader header, PagedContainerEntry entry)
        {
            using var stmCollector = new MemoryStream(entry.Length); // todo replace with byte[]

            var remainLength     = entry.Flags.HasFlag(EntryFlags.Compressed) ? entry.CompressedLength : entry.Length;
            var currentPageIndex = entry.FirstPage;
            var buff             = new byte[header.PageSize];

            while (currentPageIndex > 0)
            {
                stm.Position = currentPageIndex * header.PageSize;
                stm.Read(buff, 0, header.PageSize);

                var requestLength = remainLength > header.PageUserDataSize ? header.PageUserDataSize : remainLength;
                stmCollector.Write(buff, 0, requestLength);
                remainLength -= requestLength;

                currentPageIndex = BitConverter.ToInt32(buff, header.PageUserDataSize);
            }

            return header.DataHandler.Unpack(stmCollector.ToArray()).ToArray();
        }

        /// <summary>
        /// Read only sequence page numbers from 'startFromPage'.
        /// Last 32 bit contain next page index.
        /// Last page in sequence contain 'next page index' value == 0
        /// </summary>
        internal static int[] ReadPageSequence(this Stream stm, PagedContainerHeader header, int startFromPage)
        {
            var pages = new List<int>();

            var buff             = new byte[4];
            var currentPageIndex = startFromPage;
            while (currentPageIndex > 0)
            {
                pages.Add(currentPageIndex);

                stm.Position = currentPageIndex * header.PageSize + header.PageUserDataSize;
                stm.Read(buff, 0, 4);

                currentPageIndex = BitConverter.ToInt32(buff, 0);
            }

            return pages.ToArray();
        }
    }
}