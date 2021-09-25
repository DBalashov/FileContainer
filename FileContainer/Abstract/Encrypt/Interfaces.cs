using System;

namespace FileContainer.Encrypt
{
    interface IEncryptorDecryptor
    {
        Span<byte> Encrypt(Span<byte> data);
        Span<byte> Decrypt(Span<byte> data);
    }

    sealed class EncryptorDecryptorStub : IEncryptorDecryptor
    {
        public Span<byte> Encrypt(Span<byte> data) => data;
        public Span<byte> Decrypt(Span<byte> data) => data;
    }
}