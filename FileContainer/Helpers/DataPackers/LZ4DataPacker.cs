using System;
using FileContainer.Encrypt;
using LZ4;

namespace FileContainer
{
    sealed class LZ4DataPacker : IDataHandler
    {
        readonly IEncryptorDecryptor encryptorDecryptor;

        internal LZ4DataPacker(IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

        public Span<byte> Pack(Span<byte>   data) => encryptorDecryptor.Encrypt(data.PackLZ4());
        public Span<byte> Unpack(Span<byte> data) => encryptorDecryptor.Decrypt(data).UnpackLZ4();
    }
}