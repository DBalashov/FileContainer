using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using FileContainer;

namespace Benchmark
{
    [ExcludeFromCodeCoverage]
    sealed class TestSingleWriter : TestAbstractWriter
    {
        public TestSingleWriter(string tempTestDirectory, Dictionary<string, byte[]> extractedFiles) : base(tempTestDirectory, extractedFiles)
        {
        }

        public void Run(PersistentContainerCompressType compressType)
        {
            var totalLengths = extractedFiles.Sum(p => p.Value.Length);

            Console.WriteLine();
            Console.WriteLine("Single requests - write [Compress={0}]", compressType.ToString());
            Console.WriteLine("Page\tms\tMB\tMB/sec\tLost(%)");
            foreach (var pageSize in pageSizes)
            {
                var targetFileName = getFileNameByPageSize(tempTestDirectory, pageSize);
                try
                {
                    var sw = Stopwatch.StartNew();
                    using (var container = new PersistentContainer(targetFileName, new PersistentContainerSettings(pageSize, 0, compressType)))
                        foreach (var file in extractedFiles)
                            container.Put(file.Key, file.Value);

                    var targetFileLength = new FileInfo(targetFileName).Length;

                    sw.Stop();

                    var lengthInMB = targetFileLength / 1048576.0;
                    Console.WriteLine("{0,5}\t{1}\t{2:F1}\t{3:F1}\t{4}",
                                      pageSize, sw.ElapsedMilliseconds, lengthInMB,
                                      lengthInMB / (sw.ElapsedMilliseconds / 1000.0),
                                      compressType == PersistentContainerCompressType.None
                                          ? (((targetFileLength - totalLengths) / (double) targetFileLength) * 100).ToString("F2")
                                          : "-");
                }
                finally
                {
                    if (File.Exists(targetFileName)) File.Delete(targetFileName);
                }
            }
        }
    }
}