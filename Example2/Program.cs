using System;
using System.IO;
using System.Linq;
using FileContainer;

namespace Example2
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(Path.GetTempPath(), "test2.container");
            if (File.Exists(fileName))
                File.Delete(fileName);

            const int maxItems = 1000;
            using (var pc = new PersistentContainer(fileName, 256))
            {
                // write 0000/item0, 0000/item1, 0000/item2, ..., 0001/item10, 0001/item11, ..., 0002/item10, 0002/item21,
                foreach (var itemId in Enumerable.Range(0, maxItems))
                {
                    var path = (itemId / 10).ToString("D4");
                    pc.Put($"/{path}/item{itemId}", $"Hello, i'm item #{itemId}, some random guid: {Guid.NewGuid()}");
                }
            }
            
            using (var pc = new PersistentContainer(fileName, 256))
            {
                Console.WriteLine("File length: {0} bytes, entries: {1}", pc.Length, pc.Find().Length);
                Console.WriteLine();
                
                var mask1    = "/004?/*";
                var entries1 = pc.Find(mask1);
                Console.WriteLine("Found {0} items with mask: {1}, show first 10:", entries1.Length, mask1);
                foreach (var entry in entries1.Take(10))
                    Console.WriteLine(entry);
                Console.WriteLine();
                
                var mask2    = "/*/item?00";
                var entries2 = pc.Find(mask2);
                Console.WriteLine("Found {0} items with mask: {1}, show first 10:", entries2.Length, mask2);
                foreach (var entry in entries2.Take(10))
                    Console.WriteLine(entry);
                Console.WriteLine();
            }
        }
    }
}