using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using FileContainer;

namespace Benchmark
{
    class Program
    {
        // ~10 MB
        const string testFileUrl = "https://github.com/Leaflet/Leaflet/archive/refs/tags/v1.7.1.zip";

        protected static readonly int[] batchSizes = {8, 32, 128};

        static async Task Main(string[] args)
        {
            var tempFileName      = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var tempTestDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempTestDirectory);

            #region download test file

            Console.WriteLine("Downloading test file: {0}", testFileUrl);
            using var client = new HttpClient();
            using var fs     = new FileStream(tempFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 128 * 1024, FileOptions.DeleteOnClose);

            var stream = await client.GetStreamAsync(testFileUrl);
            await stream.CopyToAsync(fs);
            fs.Position = 0;

            var extractedFiles = extractFile(fs);
            fs.Close();

            #endregion

            var singleWriter = new TestSingleWriter(tempTestDirectory, extractedFiles);
            var batchWriter  = new TestBatchWriter(tempTestDirectory, extractedFiles);

            singleWriter.Run(PersistentContainerCompressType.None);
            foreach (var batchSize in batchSizes)
                batchWriter.Run(batchSize, PersistentContainerCompressType.None);

            singleWriter.Run(PersistentContainerCompressType.GZip);
            foreach (var batchSize in batchSizes)
                batchWriter.Run(batchSize, PersistentContainerCompressType.GZip);

            singleWriter.Run(PersistentContainerCompressType.LZ4);
            foreach (var batchSize in batchSizes)
                batchWriter.Run(batchSize, PersistentContainerCompressType.LZ4);
            
            Directory.Delete(tempTestDirectory, true);
        }

        #region extractFile

        static Dictionary<string, byte[]> extractFile(Stream stm)
        {
            Console.WriteLine("Extracting in-memory");
            using var zipFile = Ionic.Zip.ZipFile.Read(stm);
            return zipFile.Entries
                          .Where(p => !p.IsDirectory && p.UncompressedSize > 0)
                          .ToDictionary(p => p.FileName,
                                        p =>
                                        {
                                            using var stm = new MemoryStream();
                                            p.Extract(stm);
                                            return stm.ToArray();
                                        });
        }

        #endregion
    }
}