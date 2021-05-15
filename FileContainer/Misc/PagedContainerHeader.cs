using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using JetBrains.Annotations;

namespace FileContainer
{
    class PagedContainerHeader
    {
        const int HEADER_PART = 12;

        const int SIGN = 0x78213645;

        /// <summary> page size. Can be choose only while create new file. Changing of page size doesn't supported </summary>
        public readonly int PageSize;

        public readonly int PageUserDataSize;

        /// <summary> first page index of entries directory. 0 for new files, will updated after adding first entry </summary>
        public int DirectoryFirstPage;

        internal PagedContainerHeader(int pageSize)
        {
            validatePageSize(pageSize);

            PageSize         = pageSize;
            PageUserDataSize = pageSize - 4;
        }

        internal PagedContainerHeader([NotNull] Stream stm)
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
            validatePageSize(PageSize);

            DirectoryFirstPage = buff.GetInt(ref offset); // 4 byte
            if (DirectoryFirstPage < 0)
                throw new InvalidDataException($"PagedContainerHeader: DirectoryFirstPage has invalid value ({DirectoryFirstPage})");
        }

        void validatePageSize(int pageSize)
        {
            switch (pageSize)
            {
                case < HEADER_PART:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be >= {HEADER_PART} bytes (passed {pageSize} bytes)");
                case > 128 * 1024:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be <= 128 KB (passed {pageSize} bytes)");
            }
        }

        public void Write([NotNull] Stream stm)
        {
            using var bw = new BinaryWriter(stm, Encoding.Default, true);

            stm.Position = 0;
            bw.Write(SIGN);               // 4 byte
            bw.Write(PageSize);           // 4 byte
            bw.Write(DirectoryFirstPage); // 4 byte
        }

        /// <summary> возвращает количество требуемых страниц для переданного количества байтов (с вычетом служебных данных в конце страницы) </summary>
        public int GetRequiredPages(int lengthInBytes) =>
            (int) Math.Ceiling(lengthInBytes / (double) PageUserDataSize);

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"PageSize: {PageSize}";
#endif
    }
}