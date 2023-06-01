using System;
using FileContainer.Encrypt;

namespace FileContainer;

sealed class NoDataPacker : IDataPacker
{
    readonly IEncryptorDecryptor encryptorDecryptor;

    internal NoDataPacker(IEncryptorDecryptor encryptorDecryptor) => this.encryptorDecryptor = encryptorDecryptor;

    public Span<byte> Pack(Span<byte>   data) => encryptorDecryptor.Encrypt(data);
    public Span<byte> Unpack(Span<byte> data) => encryptorDecryptor.Decrypt(data);
}