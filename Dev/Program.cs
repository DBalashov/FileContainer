using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using FileContainer;

namespace Dev
{
    class Program
    {
        static void Main(string[] args)
        {
            // var cc = new Symmetric(Aes.Create());
            // for (var i = 1; i < 2000; i++)
            // {
            //     var buff = Enumerable.Range(0, i).Select(p => (byte) (p & 0xFF)).ToArray();
            //     var b    = cc.Encrypt(buff);
            //     var z    = cc.Decrypt(b);
            //     
            //     var b2 = cc.Encrypt(buff);
            //     var z2 = cc.Decrypt(b);
            // }

            var aes = Aes.Create();
            aes.GenerateKey();
            aes.GenerateIV();

            var fileName = Path.Combine(@"D:\test2.container");
            if (File.Exists(fileName))
                File.Delete(fileName);

            var text = string.Join(Environment.NewLine, Enumerable.Range(0, 15).Select(p => $"Hello, line #{p}, Текст, κείμενο, ਟੈਕਸਟ, random guid: {Guid.NewGuid()}"));

            const int maxItems = 50;
            using (var pc = new PersistentContainer(fileName, new PersistentContainerSettings(256).With(aes)))
            {
                foreach (var itemId in Enumerable.Range(0, maxItems))
                {
                    var path = (itemId / 10).ToString("D4");
                    pc.Put($"/{path}/item{itemId}", text);
                }
            }

            using (var pc = new PersistentContainer(fileName, new PersistentContainerSettings(256).With(aes)))
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

    class Symmetric
    {
        readonly SymmetricAlgorithm algo;
        readonly byte[]             tempBuffer = new byte[1024];

        internal Symmetric(SymmetricAlgorithm algo) => this.algo = algo;

        public byte[] Encrypt(byte[] data)
        {
            using var stm       = new MemoryStream();
            using var encryptor = algo.CreateEncryptor();
            using var cs        = new CryptoStream(stm, encryptor, CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return stm.ToArray();
        }

        public byte[] Decrypt(byte[] data)
        {
            using var msResult  = new MemoryStream();
            using var ms        = new MemoryStream(data);
            using var decryptor = algo.CreateDecryptor();
            using var stm       = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            int       readed;
            do
            {
                readed = stm.Read(tempBuffer, 0, tempBuffer.Length);
                if (readed > 0)
                    msResult.Write(tempBuffer, 0, readed);
            } while (readed > 0);

            return msResult.ToArray();
        }
    }
}