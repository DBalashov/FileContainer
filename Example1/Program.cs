using System;
using System.IO;
using System.Linq;
using FileContainer;

namespace Example1
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileName = @"E:\test1.binn";
            if (File.Exists(fileName))
                File.Delete(fileName);
            
            using (var kv = new PersistentContainer(fileName, 32))
            {
                // kv.Put("test1", Enumerable.Range(0, 255).Select(p => (byte) p).ToArray());
                //
                // using (var s = kv.GetStream("test1"))
                // {
                //     var buff = new byte[255];
                //
                //     s.Position = 250;
                //
                //     var x = s.Read(buff, 0, 20);
                //     var y = s.Read(buff, 20, 20);
                //}
            }

            // var fileName = @"E:\" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".binn";
            //
            // if (File.Exists(fileName))
            //     File.Delete(fileName);
            //
            // var dirName = @"D:\1";
            // var source = Directory.EnumerateDirectories(dirName, "*.*")
            //                       .Select(p => Directory.EnumerateFiles(p, "*.*")
            //                                             .ToDictionary(file => file.Substring(dirName.Length + 1).Replace('\\', '/'),
            //                                                           File.ReadAllBytes))
            //                       .ToArray();
            //
            // using (var kv = new KVFileStore(fileName))
            // {
            //     var sw = Stopwatch.StartNew();
            //     foreach (var part in source)
            //         kv.Put(part);
            //
            //     Console.WriteLine("Write: " + sw.ElapsedMilliseconds + " ms");
            // }

            //var eba           = new ExpandableBitArray(15);
            //eba[12] = eba[14] = eba[2] = true;
            //eba[31] = true;
            //eba[32] = true;
        }
    }
}