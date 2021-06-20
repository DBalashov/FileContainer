using System;
using System.Security.Cryptography;
using FileContainer.Encrypt;

namespace FileContainer
{
    public class PersistentContainerSettings
    {
        public readonly int                             PageSize;
        public readonly PersistentContainerFlags        Flags;
        public readonly PersistentContainerCompressType CompressType;

        internal IEncryptorDecryptor encryptorDecryptor = new EncryptorDecryptorStub();

        public PersistentContainerSettings(int                             pageSize     = 4096,
                                           PersistentContainerFlags        flags        = 0,
                                           PersistentContainerCompressType compressType = PersistentContainerCompressType.None)
        {
            Extenders.ValidatePageSize(pageSize);

            PageSize     = pageSize;
            Flags        = flags;
            CompressType = compressType;
        }

        public PersistentContainerSettings With(SymmetricAlgorithm algo)
        {
            encryptorDecryptor = new Symmetric(algo);
            return this;
        }
    }
}