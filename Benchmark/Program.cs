using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using FileContainer;

namespace Benchmark
{
    class Program
    {
        // https://binaries.sonarsource.com/CommercialDistribution/sonarqube-developer/sonarqube-developer-8.6.0.39681.zip
        static readonly string sourceFileName  = @"D:\sonarqube-developer-8.6.0.39681.zip";
        static readonly string targetDirectory = @"E:\";

        static readonly int[] pageSizes  = {128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536};
        static readonly int[] batchSizes = {8, 16, 32, 64, 128};

        static void Main(string[] args)
        {
            var extractedFiles = extractFile();

            testSingleWrites(extractedFiles, 0);
            foreach (var batchSize in batchSizes)
                testBatchWrites(extractedFiles, batchSize, 0);

            testSingleWrites(extractedFiles, PersistentContainerFlags.Compressed);
            foreach (var batchSize in batchSizes)
                testBatchWrites(extractedFiles, batchSize, PersistentContainerFlags.Compressed);

            foreach (var pageSize in pageSizes)
            {
                var fileName = getFileNameByPageSize(pageSize);
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        #region extractFile

        static Dictionary<string, byte[]> extractFile()
        {
            Console.WriteLine("Extracting");
            using var zipFile = Ionic.Zip.ZipFile.Read(sourceFileName);
            return zipFile.Entries
                          .Where(p => !p.IsDirectory)
                          .ToDictionary(p => p.FileName,
                                        p =>
                                        {
                                            using var stm = new MemoryStream();
                                            p.Extract(stm);
                                            return stm.ToArray();
                                        });
        }

        #endregion

        #region test - single writes

        static void testSingleWrites(Dictionary<string, byte[]> extractedFiles, PersistentContainerFlags flags)
        {
            var totalLengths = extractedFiles.Sum(p => p.Value.Length);

            Console.WriteLine("Single requests - write [{0}]", flags);
            Console.WriteLine("Page\tms\tMB\tMB/sec\tLost(%)");
            foreach (var pageSize in pageSizes)
            {
                var targetFileName = getFileNameByPageSize(pageSize);
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);

                var sw = Stopwatch.StartNew();
                using (var container = new PersistentContainer(targetFileName,new PersistentContainerSettings( pageSize, flags)))
                    foreach (var file in extractedFiles)
                        container.Put(file.Key, file.Value);

                var targetFileLength = new FileInfo(targetFileName).Length;

                sw.Stop();

                var lengthInMB = targetFileLength / 1048576.0;
                Console.WriteLine("{0,5}\t{1}\t{2:F1}\t{3:F1}\t{4}",
                                  pageSize, sw.ElapsedMilliseconds, lengthInMB,
                                  lengthInMB / (sw.ElapsedMilliseconds / 1000.0),
                                  flags == 0
                                      ? (((targetFileLength - totalLengths) / (double) targetFileLength) * 100).ToString("F2")
                                      : "-");
            }
        }

        #endregion

        #region test - batch writes

        static void testBatchWrites(Dictionary<string, byte[]> extractedFiles, int batchSize, PersistentContainerFlags flags)
        {
            Console.WriteLine();
            Console.WriteLine("Batch requests [{0}]: {1}", flags == 0 ? "-" : flags.ToString(), batchSize);
            Console.WriteLine("Page        ms     MB/sec(w)    MB/sec(r)     Ratio");

            var items = makeBatches(extractedFiles, batchSize);
            foreach (var pageSize in pageSizes)
            {
                var targetFileName = getFileNameByPageSize(pageSize);
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);

                var sw = Stopwatch.StartNew();
                using (var container = new PersistentContainer(targetFileName, new PersistentContainerSettings(pageSize, flags)))
                    foreach (var item in items)
                        container.Put(item);

                var elapsedWrites = sw.ElapsedMilliseconds;

                var targetFileLength = new FileInfo(targetFileName).Length;

                sw.Restart();
                var rawLength        = 0;
                var compressedLength = 0;
                using (var container = new PersistentContainer(targetFileName, new PersistentContainerSettings(pageSize, flags)))
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
                var rawLengthInMB = rawLength / 1048576.0;
                Console.WriteLine("{0,5} {1,8}      {2,8:F1}     {3,8:F1} {4,8:F2}x",
                                  pageSize, sw.ElapsedMilliseconds,
                                  lengthInMB / (elapsedWrites / 1000.0),
                                  rawLengthInMB / (elapsedRead / 1000.0),
                                  rawLength / (double) compressedLength);
            }
        }

        static List<Dictionary<string, byte[]>> makeBatches(Dictionary<string, byte[]> items, int elementCount)
        {
            var r   = new List<Dictionary<string, byte[]>>();
            var arr = items.ToArray();
            while (arr.Any())
            {
                r.Add(arr.Take(elementCount).ToDictionary(p => p.Key, p => p.Value));
                arr = arr.Skip(elementCount).ToArray();
            }

            return r;
        }

        #endregion

        static string getFileNameByPageSize(int pageSize) =>
            Path.Combine(targetDirectory, pageSize.ToString().PadLeft(5, '_') + ".binn");
    }
}