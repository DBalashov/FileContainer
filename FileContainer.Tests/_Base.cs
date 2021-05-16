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
                ["dir/file01.txt"] = getRandomBytes(pageSize - 1), // (pageSize-4)-1  => 1 страница (1 байт свободен на странице)
                ["dir/file02.txt"] = getRandomBytes(pageSize),     // (pageSize-4)    => 1 страница (точное совпадение)
                ["dir/file03.txt"] = getRandomBytes(pageSize + 1), // (pageSize-4)+1  => 2 страница (1 байт на последней странице)

                ["dir/fileA4.txt"] = getRandomBytes(pageSize * 2 - 1), // 2*(pageSize-4)-1   => 2 страницы  (1 байт свободен на последней странице)
                ["dir/fileA5.txt"] = getRandomBytes(pageSize * 2),     // 2*(pageSize-4)     => 2 страницы (точное совпадение)
                ["dir/fileA6.txt"] = getRandomBytes(pageSize * 2 + 1), // 2*(pageSize-4)+1   => 3 страницы (1 байт на последней странице)

                ["dir/fileB4.txt"] = getRandomBytes(pageSize * 100 - 1), // 100*(pageSize-4)-1   => 100 страницы (1 байт свободен на последней странице)
                ["dir/fileB5.txt"] = getRandomBytes(pageSize * 100),     // 100*(pageSize-4)     => 100 страниц (точное совпадение)
                ["dir/fileB6.txt"] = getRandomBytes(pageSize * 100 + 1), // 100*(pageSize-4)+1   => 101 страница (1 байт на последней странице)
            };

        protected string                       fileName;
        readonly  List<PagedContainerAbstract> stores = new();

        protected void DoIt(Action<Func<PagedContainerAbstract>> action)
        {
            foreach (var pageSize in new[] {32, 4096}) // тест на два размера страниц (маленький и большой): 32 и 4096 байт
                try
                {
                    fileName = Path.Combine(Path.GetTempPath(), "_Reads.kv");
                    if (File.Exists(fileName))
                        File.Delete(fileName);

                    action(() =>
                    {
                        var s = new PersistentContainer(fileName, new PersistentContainerSettings(pageSize));
                        stores.Add(s);
                        return s;
                    });
                    

                    var stm = new MemoryStream();
                    action(() =>
                    {
                        var s = new InMemoryContainer(stm, new PersistentContainerSettings(pageSize));
                        stores.Add(s);
                        return s;
                    });
                    
                }
                finally
                {
                    foreach (var s in stores)
                        s.Dispose();
                    stores.Clear();

                    if (File.Exists(fileName))
                        File.Delete(fileName);
                }
        }
    }
}