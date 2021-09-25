using System;
using System.IO;
using System.Security.Cryptography;

namespace FileContainer.Encrypt
{
    sealed class Symmetric : IEncryptorDecryptor
    {
        readonly SymmetricAlgorithm algo;

        internal Symmetric(SymmetricAlgorithm algo) => this.algo = algo;

        public Span<byte> Encrypt(Span<byte> data)
        {
            using var stm = new MemoryStream();
            stm.Write(BitConverter.GetBytes(data.Length), 0, 4);

            using (var encryptor = algo.CreateEncryptor())
            using (var cs = new CryptoStream(stm, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(data);
                cs.FlushFinalBlock();
                cs.Flush();
                data = stm.ToArray();
            }

            return data;
        }

        public Span<byte> Decrypt(Span<byte> data)
        {
            var resultDataLength = BitConverter.ToInt32(data);
            var buff             = new byte[resultDataLength];

            using var ms = new MemoryStream();
            ms.Write(data.Slice(4));
            ms.Position = 0;

            using (var decryptor = algo.CreateDecryptor())
            using (var stm = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                stm.Read(buff, 0, resultDataLength);

            return buff;
        }
    }
}