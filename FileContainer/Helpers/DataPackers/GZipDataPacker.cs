using System;
using System.IO.Compression;
using FileContainer.Encrypt;

namespace FileContainer;

sealed class GZipDataPacker : IDataPacker
{
    readonly byte[] buff = new byte[8192];

    readonly IEncryptorDecryptor encryptorDecryptor;

    internal GZipDataPacker(IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

    public Span<byte> Pack(Span<byte> data)
    {
        using var stm = MemoryStreamManager.Instance.GetStream();
        using (var gz = new GZipStream(stm, CompressionMode.Compress, true))
        {
            gz.Write(data);
            gz.Flush();
        }

        return encryptorDecryptor.Encrypt(stm.GetBuffer().AsSpan(0, (int)stm.Length));
    }

    public Span<byte> Unpack(Span<byte> data)
    {
        using var stm = MemoryStreamManager.Instance.GetStream();
        using (var st1 = MemoryStreamManager.Instance.GetStream())
        {
            st1.Write(encryptorDecryptor.Decrypt(data));
            st1.Position = 0;

            using (var gz = new GZipStream(st1, CompressionMode.Decompress, true))
            {
                gz.CopyTo(stm);
                gz.Close();
            }

            st1.Close();
        }

        return stm.GetBuffer().AsSpan(0, (int)stm.Length).ToArray();
    }
}