using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FileContainer;

namespace Example1
{
    [ExcludeFromCodeCoverage]
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = Path.Combine(Path.GetTempPath(), "test.container");
            if (File.Exists(fileName))
                File.Delete(fileName);

            using (var pc = new PersistentContainer(fileName, new PersistentContainerSettings(256)))
            {
                pc.Put("item1", "Hello");
                pc.Put("item2", "User");
                
                // overwrite item1:
                pc.Put("item1", "Bye!");

                pc.Append("item1", "See you later!");

                pc.Delete("item2");

                pc.Put("item3", "another item");
            }

            using (var pc = new PersistentContainer(fileName, new PersistentContainerSettings(256)))
            {
                var entries = pc.Find();
                foreach (var entry in entries)
                    Console.WriteLine(entry);
                
                Console.WriteLine();
                foreach (var entry in entries)
                    Console.WriteLine("{0}: {1}", entry.Name, pc.GetString(entry.Name));
            }
        }
    }
}