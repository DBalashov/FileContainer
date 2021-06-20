using FileContainer.Encrypt;
using JetBrains.Annotations;

namespace FileContainer
{
    class NoDataPacker : IDataHandler
    {
        [NotNull] readonly IEncryptorDecryptor encryptorDecryptor;

        internal NoDataPacker([NotNull] IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

        public byte[] Pack(byte[]   data) => encryptorDecryptor.Encrypt(data);
        public byte[] Unpack(byte[] data) => encryptorDecryptor.Decrypt(data);
    }

}