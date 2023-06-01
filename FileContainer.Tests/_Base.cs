using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FileContainer.Tests
{
    public abstract class TestBase
    {
        static readonly Random r = new(Guid.NewGuid().GetHashCode());

        protected byte[] getRandomBytes(int count) => Enumerable.Range(0, count).Select(p => (byte) r.Next(255)).ToArray();

        protected Dictionary<string, byte[]> getRandomBlocks(int pageSize) =>
            new()
            {
                ["dir/file01.txt"] = getRandomBytes(pageSize - 1), // (pageSize-4)-1  => 1 page (1 byte free at page)
                ["dir/file02.txt"] = getRandomBytes(pageSize),     // (pageSize-4)    => 1 page (exact lengths)
                ["dir/file03.txt"] = getRandomBytes(pageSize + 1), // (pageSize-4)+1  => 2 page (1 byte at last page)

                ["dir/fileA4.txt"] = getRandomBytes(pageSize * 2 - 1), // 2*(pageSize-4)-1   => 2 pages (1 byte free at page)
                ["dir/fileA5.txt"] = getRandomBytes(pageSize * 2),     // 2*(pageSize-4)     => 2 pages (exact lengths)
                ["dir/fileA6.txt"] = getRandomBytes(pageSize * 2 + 1), // 2*(pageSize-4)+1   => 3 pages (1 byte at last page)

                ["dir/fileB4.txt"] = getRandomBytes(pageSize * 100 - 1), // 100*(pageSize-4)-1   => 100 pages (1 byte free at page)
                ["dir/fileB5.txt"] = getRandomBytes(pageSize * 100),     // 100*(pageSize-4)     => 100 pages (exact lengths)
                ["dir/fileB6.txt"] = getRandomBytes(pageSize * 100 + 1), // 100*(pageSize-4)+1   => 101 pages (1 byte at last page)
            };

        protected void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0)
        {
            var stores = new List<PagedContainerAbstract>();
            
            // test with two page sizes (small & large)
            foreach (var pageSize in new[] {PagedContainerAbstract.MinPageSize, PagedContainerAbstract.MinPageSize * 8})
            {
                var file1 = Path.GetTempFileName();
                try
                {
                    // file, non encrypted
                    action(() =>
                           {
                               var s = new PersistentContainer(file1, new PersistentContainerSettings(pageSize, flags, compressType));
                               stores.Add(s);
                               return s;
                           });

                    // in memory, non encrypted
                    using (var stmNonEncrypted = new MemoryStream())
                        action(() =>
                               {
                                   var s = new InMemoryContainer(stmNonEncrypted, new PersistentContainerSettings(pageSize, flags, compressType));
                                   stores.Add(s);
                                   return s;
                               });
                }
                finally
                {
                    try
                    {
                        foreach (var s in stores) s.Dispose();
                        stores.Clear();
                        if (File.Exists(file1)) File.Delete(file1);
                    }
                    catch
                    {
                        
                    }
                }
            }
        }
    }
}