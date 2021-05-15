using System;
using System.IO;
using System.Linq;
using FileContainer;

namespace Dev
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(@"D:\test2.container");
            if (File.Exists(fileName))
                File.Delete(fileName);

            var text = string.Join(Environment.NewLine, Enumerable.Range(0, 15).Select(p => $"Hello, line #{p}, Текст, κείμενο, ਟੈਕਸਟ, random guid: {Guid.NewGuid()}"));

            const int maxItems = 1000;
            using (var pc = new PersistentContainer(fileName, 256, PersistentContainerFlags.Compressed))
            {
                foreach (var itemId in Enumerable.Range(0, maxItems))
                {
                    var path = (itemId / 10).ToString("D4");
                    pc.Put($"/{path}/item{itemId}", text);
                }
            }

            using (var pc = new PersistentContainer(fileName, 256))
            {
                Console.WriteLine("File length: {0} bytes, entries: {1}", pc.Length, pc.Find().Length);
                Console.WriteLine();

                var mask    = "/004?/*";
                var entries = pc.Find(mask);
                Console.WriteLine("Found {0} items with mask: {1}, show first 10:", entries.Length, mask);
                foreach (var entry in entries.Take(10))
                {
                    var item = pc.GetString(entry.Name);
                    Console.WriteLine(entry);
                }
            }
        }
    }
}