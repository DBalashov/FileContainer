using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FileContainer;

namespace Benchmark
{
    [ExcludeFromCodeCoverage]
    sealed class TestBatchWriter : TestAbstractWriter
    {
        public TestBatchWriter(string tempTestDirectory, Dictionary<string, byte[]> extractedFiles) : base(tempTestDirectory, extractedFiles)
        {
        }

        public void Run(int batchSize, PersistentContainerCompressType compressType)
        {
            Console.WriteLine();
            Console.WriteLine("Batch requests [Compress={0}]: {1}", compressType.ToString(), batchSize);
            Console.WriteLine("Page        ms     MB/sec(w)    MB/sec(r)     Ratio");

            var items = makeBatches(extractedFiles, batchSize);
            foreach (var pageSize in pageSizes)
            {
                var targetFileName = getFileNameByPageSize(tempTestDirectory, pageSize);
                try
                {
                    var sw = Stopwatch.StartNew();
                    using (var container = new PersistentContainer(targetFileName, new PersistentContainerSettings(pageSize, 0, compressType)))
                        foreach (var item in items)
                            container.Put(item);

                    var elapsedWrites = sw.ElapsedMilliseconds;

                    var targetFileLength = new FileInfo(targetFileName).Length;

                    sw.Restart();
                    var rawLength        = 0;
                    var compressedLength = 0;
                    using (var container = new PersistentContainer(targetFileName, new PersistentContainerSettings(pageSize, 0, compressType)))
                    {
                        foreach (var entry in container.Find())
                        {
                            compressedLength += entry.CompressedLength;
                            rawLength        += container.Get(entry.Name)?.Length ?? 0;
                        }
                    }

                    var elapsedRead = sw.ElapsedMilliseconds;
                    sw.Stop();

                    var lengthInMB    = targetFileLength / 1048576.0;
                    var rawLengthInMB = rawLength        / 1048576.0;
                    Console.WriteLine("{0,5} {1,8}      {2,8:F1}     {3,8:F1} {4,8:F2}x",
                                      pageSize, sw.ElapsedMilliseconds,
                                      lengthInMB    / (elapsedWrites / 1000.0),
                                      rawLengthInMB / (elapsedRead   / 1000.0),
                                      rawLength     / (double) compressedLength);
                }
                finally
                {
                    if (File.Exists(targetFileName)) File.Delete(targetFileName);
                }
            }
        }
    }
}