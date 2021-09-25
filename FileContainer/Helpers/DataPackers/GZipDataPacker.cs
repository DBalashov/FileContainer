using System;
using System.IO;
using System.IO.Compression;
using FileContainer.Encrypt;

namespace FileContainer
{
    sealed class GZipDataPacker : IDataHandler
    {
        readonly byte[] buff = new byte[8192];

        readonly IEncryptorDecryptor encryptorDecryptor;

        internal GZipDataPacker(IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

        public Span<byte> Pack(Span<byte> data)
        {
            using var stm = new MemoryStream();
            using (var gz = new GZipStream(stm, CompressionMode.Compress, true))
            {
                gz.Write(data);
                gz.Flush();
            }

            return encryptorDecryptor.Encrypt(stm.ToArray());
        }

        public Span<byte> Unpack(Span<byte> data)
        {
            using var stm = new MemoryStream();
            using (var st1 = new MemoryStream())
            {
                st1.Write(encryptorDecryptor.Decrypt(data));
                st1.Position = 0;
                
                using (var gz = new GZipStream(st1, CompressionMode.Decompress, true))
                {
                    int read;
                    while ((read = gz.Read(buff, 0, buff.Length)) > 0)
                        stm.Write(buff, 0, read);
                    gz.Close();
                }

                st1.Close();
            }

            return stm.ToArray();
        }
    }
}