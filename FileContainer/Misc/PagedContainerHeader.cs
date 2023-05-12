using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using FileContainer.Encrypt;
using SpanByteExtenders;

namespace FileContainer
{
    class PagedContainerHeader
    {
        /// <summary> header size less than minimal page size </summary>
        internal const int HEADER_PART = 16;

        internal const int MIN_PAGE_SIZE = 256;
        internal const int MAX_PAGE_SIZE = 256 * 1024;

        const int SIGN = 0x78213645;

        /// <summary> page size. Can be choose only while create new file. Changing of page size doesn't supported </summary>
        public readonly int PageSize;

        public readonly int PageUserDataSize;

        public readonly PersistentContainerFlags        Flags;
        public readonly PersistentContainerCompressType CompressType;

        /// <summary> first page index of entries directory. 0 for new files, will updated after adding first entry </summary>
        public int DirectoryFirstPage;

        public readonly IDataHandler DataHandler;

        #region constructor

        internal PagedContainerHeader(PersistentContainerSettings settings)
        {
            PageSize         = settings.PageSize;
            PageUserDataSize = settings.PageSize - 4;
            Flags            = settings.Flags;
            CompressType     = settings.CompressType;
            DataHandler      = getDataHandler(settings.CompressType, settings.encryptorDecryptor);
        }

        internal PagedContainerHeader(Stream stm, IEncryptorDecryptor encryptorDecryptor)
        {
            if (stm.Length < HEADER_PART)
                throw new InvalidDataException("PagedContainerHeader: File corrupted (too small)");

            stm.Position = 0;

            var buff = new byte[HEADER_PART];
            stm.Read(buff, 0, buff.Length);

            var span   = buff.AsSpan();
            
            var sign   = span.ReadInt32();
            if (sign != SIGN)
                throw new InvalidDataException($"PagedContainerHeader: File corruped (signature {sign:X}h invalid, must be {SIGN:X}h)");

            PageSize         = span.ReadInt32();
            PageUserDataSize = PageSize - 4;
            Extenders.ValidatePageSize(PageSize);

            DirectoryFirstPage = span.ReadInt32();
            if (DirectoryFirstPage < 0)
                throw new InvalidDataException($"PagedContainerHeader: DirectoryFirstPage has invalid value ({DirectoryFirstPage})");

            Flags = (PersistentContainerFlags) span.ReadUInt16();

            var flagsData = span.ReadByte();
            CompressType = (PersistentContainerCompressType) flagsData;
            DataHandler  = getDataHandler(CompressType, encryptorDecryptor);
        }


        IDataHandler getDataHandler(PersistentContainerCompressType compressType, IEncryptorDecryptor encryptorDecryptor) =>
            compressType switch
            {
                PersistentContainerCompressType.None => new NoDataPacker(encryptorDecryptor),
                PersistentContainerCompressType.GZip => new GZipDataPacker(encryptorDecryptor),
                PersistentContainerCompressType.LZ4  => new LZ4DataPacker(encryptorDecryptor),
                _                                    => throw new InvalidDataException("Unsupported compress type: " + compressType)
            };

        #endregion

        public void Write(Stream stm)
        {
            using var bw = new BinaryWriter(stm, Encoding.Default, true);

            stm.Position = 0;
            bw.Write(SIGN);               // 4 byte
            bw.Write(PageSize);           // 4 byte
            bw.Write(DirectoryFirstPage); // 4 byte

            bw.Write((ushort) ((int) Flags & 0xFFFF)); // 2 byte

            bw.Write((byte) ((int) CompressType & 0xFF)); // 1 byte
            bw.Write(0);                                  // 1 byte
        }

        /// <summary> return required page count for store lengthInBytes (including internal data size at end of each page) </summary>
        public int GetRequiredPages(int lengthInBytes) =>
            (int) Math.Ceiling(lengthInBytes / (double) PageUserDataSize);

        [ExcludeFromCodeCoverage]
        public override string ToString() => $"PageSize: {PageSize}, Flags: {Flags}, CompressType: {CompressType}";
    }
}