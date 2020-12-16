using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    static class ReadExtenders
    {
        /// <summary> Read data and page numbers, starting from 'startFromPage' </summary>
        internal static PageSequence ReadWithPageSequence([NotNull] this Stream stm, [NotNull] FileContainerHeader header, int startFromPage)
        {
            using var stmCollector = new MemoryStream();

            var pages = new List<int>();
            var buff  = new byte[header.PageSize];

            while (startFromPage > 0)
            {
                pages.Add(startFromPage);
                stm.Position = startFromPage * header.PageSize;
                stm.Read(buff, 0, header.PageSize);

                stmCollector.Write(buff, 0, header.PageUserDataSize); // requestLength
                startFromPage = BitConverter.ToInt32(buff, header.PageUserDataSize);
            }

            return new PageSequence(stmCollector.ToArray(), pages.ToArray());
        }

        /// <summary> Read data from pages, starting with entry.FirstPage </summary>
        [NotNull]
        internal static byte[] ReadEntryPageSequence([NotNull] this Stream stm, [NotNull] FileContainerHeader header, [NotNull] FileContainerEntry entry)
        {
            using var stmCollector = new MemoryStream(entry.Length); // todo replace with byte[]

            var buff         = new byte[header.PageSize];
            var remainLength = entry.Length;

            var currentPageIndex = entry.FirstPage;
            while (currentPageIndex > 0)
            {
                stm.Position = currentPageIndex * header.PageSize;
                stm.Read(buff, 0, header.PageSize);

                var requestLength = remainLength > header.PageUserDataSize ? header.PageUserDataSize : remainLength;
                stmCollector.Write(buff, 0, requestLength);
                remainLength -= requestLength;

                currentPageIndex = BitConverter.ToInt32(buff, header.PageUserDataSize);
            }

            return stmCollector.ToArray();
        }

        /// <summary>
        /// Read only sequence page numbers from 'startFromPage'.
        /// Last 32 bit contain next page index.
        /// Last page in sequence contain 'next page index' value == 0
        /// </summary>
        internal static int[] ReadPageSequence([NotNull] this Stream stm, [NotNull] FileContainerHeader header, int startFromPage)
        {
            var pages = new List<int>();

            var buff = new byte[4];
            while (startFromPage > 0)
            {
                pages.Add(startFromPage);

                stm.Position = startFromPage * header.PageSize + header.PageUserDataSize;
                stm.Read(buff, 0, 4);

                startFromPage = BitConverter.ToInt32(buff, 0);
            }

            return pages.ToArray();
        }
    }
    
    readonly struct PageSequence
    {
        [NotNull] internal readonly byte[] Data;
        [NotNull] internal readonly int[]  Pages;

        internal PageSequence([NotNull] byte[] data, [NotNull] int[] pages)
        {
            Data  = data;
            Pages = pages;
        }
    }
}