using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FileContainer.Encrypt;
using JetBrains.Annotations;

namespace FileContainer
{
    class PagedContainerHeader
    {
        /// <summary> header size == minimal page size </summary>
        internal const int HEADER_PART = 16;

        const int SIGN = 0x78213645;

        /// <summary> page size. Can be choose only while create new file. Changing of page size doesn't supported </summary>
        public readonly int PageSize;

        public readonly int PageUserDataSize;

        public readonly PersistentContainerFlags Flags;

        /// <summary> first page index of entries directory. 0 for new files, will updated after adding first entry </summary>
        public int DirectoryFirstPage;

        [NotNull] public IDataHandler DataHandler;

        #region constructor

        internal PagedContainerHeader([NotNull] PersistentContainerSettings settings)
        {
            PageSize         = settings.PageSize;
            PageUserDataSize = settings.PageSize - 4;
            Flags            = settings.Flags;
            DataHandler = Flags.HasFlag(PersistentContainerFlags.Compressed)
                ? new GZipDataPacker(settings.encryptorDecryptor)
                : new NoDataPacker(settings.encryptorDecryptor);
        }

        internal PagedContainerHeader([NotNull] PersistentContainerSettings settings, [NotNull] Stream stm)
        {
            if (stm.Length < HEADER_PART)
                throw new InvalidDataException("PagedContainerHeader: File corrupted (too small)");

            stm.Position = 0;

            var buff = new byte[HEADER_PART];
            stm.Read(buff, 0, buff.Length);

            int offset = 0;
            var sign   = buff.GetInt(ref offset); // 4 byte
            if (sign != SIGN)
                throw new InvalidDataException($"PagedContainerHeader: File corruped (signature {sign:X}h invalid, must be {SIGN:X}h)");

            PageSize         = buff.GetInt(ref offset); // 4 byte
            PageUserDataSize = PageSize - 4;
            Extenders.ValidatePageSize(PageSize);

            DirectoryFirstPage = buff.GetInt(ref offset); // 4 byte
            if (DirectoryFirstPage < 0)
                throw new InvalidDataException($"PagedContainerHeader: DirectoryFirstPage has invalid value ({DirectoryFirstPage})");

            Flags = (PersistentContainerFlags) buff.GetInt(ref offset);
            DataHandler = Flags.HasFlag(PersistentContainerFlags.Compressed)
                ? new GZipDataPacker(settings.encryptorDecryptor)
                : new NoDataPacker(settings.encryptorDecryptor);
        }

        #endregion

        public void Write([NotNull] Stream stm)
        {
            using var bw = new BinaryWriter(stm, Encoding.Default, true);

            stm.Position = 0;
            bw.Write(SIGN);               // 4 byte
            bw.Write(PageSize);           // 4 byte
            bw.Write(DirectoryFirstPage); // 4 byte
            bw.Write((int) Flags);        // 4 byte
        }

        /// <summary> return required page count for store lengthInBytes (including internal data size at end of each page) </summary>
        public int GetRequiredPages(int lengthInBytes) =>
            (int) Math.Ceiling(lengthInBytes / (double) PageUserDataSize);

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"PageSize: {PageSize}";
#endif
    }
}