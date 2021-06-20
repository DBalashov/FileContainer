using JetBrains.Annotations;

namespace FileContainer.Encrypt
{
    interface IEncryptorDecryptor
    {
        [NotNull]
        byte[] Encrypt([NotNull] byte[] data);

        [NotNull]
        byte[] Decrypt([NotNull] byte[] data);
    }

    class EncryptorDecryptorStub : IEncryptorDecryptor
    {
        public byte[] Encrypt(byte[] data) => data;

        public byte[] Decrypt(byte[] data) => data;
    }
}