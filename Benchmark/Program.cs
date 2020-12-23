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
        static void Main(string[] args)
        {
            // https://binaries.sonarsource.com/CommercialDistribution/sonarqube-developer/sonarqube-developer-8.6.0.39681.zip
            var sourceFileName  = @"D:\Downloads\sonarqube-developer-8.6.0.39681.zip";
            var targetDirectory = @"E:\";
            var pageSizes       = new[] {128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536};
            var batchSizes      = new[] {8, 16, 32, 64, 128};

            Dictionary<string, byte[]> extractedFiles;

            Console.WriteLine("Extracting");
            using (var zipFile = Ionic.Zip.ZipFile.Read(sourceFileName))
                extractedFiles = zipFile.Entries
                                        .Where(p => !p.IsDirectory)
                                        .ToDictionary(p => p.FileName,
                                                      p =>
                                                      {
                                                          using var stm = new MemoryStream();
                                                          p.Extract(stm);
                                                          return stm.ToArray();
                                                      });

            var totalLengths = extractedFiles.Sum(p => p.Value.Length);

            Console.WriteLine("Single requests - write");
            Console.WriteLine("Page\tms\tMB\tMB/sec\tLost(%)");
            foreach (var pageSize in pageSizes)
            {
                var targetFileName = Path.Combine(targetDirectory, pageSize.ToString().PadLeft(5, '_') + ".binn");
                if (File.Exists(targetFileName))
                    File.Delete(targetFileName);

                var sw = Stopwatch.StartNew();
                using (var container = new PersistentContainer(targetFileName, pageSize))
                    foreach (var file in extractedFiles)
                        container.Put(file.Key, file.Value);

                var targetFileLength = new FileInfo(targetFileName).Length;

                sw.Stop();

                var lengthInMB = targetFileLength / 1048576.0;
                Console.WriteLine("{0,5}\t{1}\t{2:F1}\t{3:F1}\t{4:F2}",
                                  pageSize, sw.ElapsedMilliseconds, lengthInMB,
                                  lengthInMB / (sw.ElapsedMilliseconds / 1000.0),
                                  ((targetFileLength - totalLengths) / (double) targetFileLength) * 100);
            }

            foreach (var batchSize in batchSizes)
            {
                Console.WriteLine();
                Console.WriteLine("Batch requests - batch: {0}", batchSize);
                Console.WriteLine("Page\tms\tMB/sec");

                var items = makeBatches(extractedFiles, batchSize);
                foreach (var pageSize in pageSizes)
                {
                    var targetFileName = Path.Combine(targetDirectory, pageSize.ToString().PadLeft(5, '_') + ".binn");
                    if (File.Exists(targetFileName))
                        File.Delete(targetFileName);

                    var sw = Stopwatch.StartNew();
                    using (var container = new PersistentContainer(targetFileName, pageSize))
                        foreach (var item in items)
                            container.Put(item);

                    var targetFileLength = new FileInfo(targetFileName).Length;

                    sw.Stop();

                    var lengthInMB = targetFileLength / 1048576.0;
                    Console.WriteLine("{0,5}\t{1}\t{2:F1}",
                                      pageSize, sw.ElapsedMilliseconds,
                                      lengthInMB / (sw.ElapsedMilliseconds / 1000.0));
                }
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
    }
}