using FileContainer.Encrypt;
using JetBrains.Annotations;
using LZ4;

namespace FileContainer
{
    class LZ4DataPacker : IDataHandler
    {
        [NotNull] readonly IEncryptorDecryptor encryptorDecryptor;

        internal LZ4DataPacker([NotNull] IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

        public byte[] Pack(byte[] data) => 
            encryptorDecryptor.Encrypt(LZ4Codec.Pack(data, 0, data.Length));

        public byte[] Unpack(byte[] data) => LZ4Codec.Unpack(encryptorDecryptor.Decrypt(data));
    }
}