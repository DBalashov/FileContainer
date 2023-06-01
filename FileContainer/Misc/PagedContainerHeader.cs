using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using FileContainer.Encrypt;
using SpanByteExtenders;

namespace FileContainer;

sealed class PagedContainerHeader
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

    public readonly IDataPacker DataHandler;

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
            throw new InvalidDataException("PagedContainerHeader: Data corrupted (too small)");

        stm.Position = 0;

        var buff = new byte[HEADER_PART];
        var read = stm.Read(buff, 0, buff.Length);
        if (read < buff.Length)
            throw new InvalidDataException("PagedContainerHeader: Data corruped (can't read header)");

        var span = buff.AsSpan();

        var sign = span.Read<int>();
        if (sign != SIGN)
            throw new InvalidDataException($"PagedContainerHeader: Data corruped (signature {sign:X}h invalid, must be {SIGN:X}h)");

        PageSize         = span.Read<int>();
        PageUserDataSize = PageSize - 4;
        Extenders.ValidatePageSize(PageSize);

        DirectoryFirstPage = span.Read<int>();
        if (DirectoryFirstPage < 0)
            throw new InvalidDataException($"PagedContainerHeader: DirectoryFirstPage has invalid value ({DirectoryFirstPage})");

        Flags = (PersistentContainerFlags) span.Read<ushort>();

        var flagsData = span.Read<byte>();
        CompressType = (PersistentContainerCompressType) flagsData;
        DataHandler  = getDataHandler(CompressType, encryptorDecryptor);
    }


    IDataPacker getDataHandler(PersistentContainerCompressType compressType, IEncryptorDecryptor encryptorDecryptor) =>
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
        var buff = new byte[4 + 4 + 4 + 2 + 1 + 1];
        var span = buff.AsSpan();
        span.Write(SIGN);                                 // 4 byte
        span.Write(PageSize);                             // 4 byte
        span.Write(DirectoryFirstPage);                   // 4 byte
        span.Write((ushort) ((int) Flags      & 0xFFFF)); // 2 byte
        span.Write((byte) ((int) CompressType & 0xFF));   // 1 byte
        span.Write((byte) 0);                             // 1 byte

        stm.Position = 0;
        stm.Write(buff, 0, buff.Length);
    }

    /// <summary> return required page count for store lengthInBytes (including internal data size at end of each page) </summary>
    public int GetRequiredPages(int lengthInBytes) =>
        (int) Math.Ceiling(lengthInBytes / (double) PageUserDataSize);

    [ExcludeFromCodeCoverage]
    public override string ToString() => $"PageSize: {PageSize}, Flags: {Flags}, CompressType: {CompressType}";
}