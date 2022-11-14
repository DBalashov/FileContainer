using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Benchmark
{
    abstract class TestAbstractWriter
    {
        // static readonly int[] pageSizes  = { 128, 256, 512, 1024, 2048, 4096, 8192, 16384, 32768, 65536 };
        protected static readonly int[] pageSizes  = {128, 512, 2048, 8192, 32768};

        protected readonly string                          tempTestDirectory;
        protected readonly Dictionary<string, byte[]>      extractedFiles;

        protected  TestAbstractWriter(string tempTestDirectory, Dictionary<string, byte[]> extractedFiles)
        {
            this.tempTestDirectory = tempTestDirectory;
            this.extractedFiles    = extractedFiles;
        }
        
        protected static List<Dictionary<string, byte[]>> makeBatches(Dictionary<string, byte[]> items, int elementCount)
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

        protected static string getFileNameByPageSize(string targetDirectory, int pageSize) =>
            Path.Combine(targetDirectory, pageSize.ToString().PadLeft(5, '_') + ".binn");
    }
}