using System;
using System.Security.Cryptography;
using FileContainer.Encrypt;

namespace FileContainer
{
    public class PersistentContainerSettings
    {
        public readonly int                      PageSize;
        public readonly PersistentContainerFlags Flags;

        internal IEncryptorDecryptor encryptorDecryptor = new EncryptorDecryptorStub();

        public PersistentContainerSettings(int pageSize = 4096, PersistentContainerFlags flags = 0)
        {
            Extenders.ValidatePageSize(pageSize);

            PageSize = pageSize;
            Flags    = flags;
        }

        // public PersistentContainerSettings With(AsymmetricAlgorithm encryptor)
        // {
        //     return this;
        // }

        public PersistentContainerSettings With(SymmetricAlgorithm algo)
        {
            encryptorDecryptor = new Symmetric(algo);
            return this;
        }
    }
}