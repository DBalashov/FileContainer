using System;
using System.Security.Cryptography;

namespace FileContainer
{
    public class PersistentContainerSettings
    {
        public readonly int                      PageSize;
        public readonly PersistentContainerFlags Flags;

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
        //
        // public PersistentContainerSettings With(SymmetricAlgorithm encryptor)
        // {
        //     
        //     return this;
        // }
    }
}