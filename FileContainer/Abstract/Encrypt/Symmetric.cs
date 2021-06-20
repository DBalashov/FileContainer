using System.IO;
using System.Security.Cryptography;

namespace FileContainer.Encrypt
{
    class Symmetric : IEncryptorDecryptor
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