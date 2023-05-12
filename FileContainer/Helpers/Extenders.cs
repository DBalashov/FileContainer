using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FileContainer
{
    static class Extenders
    {
        static readonly Encoding defaultEncoding = Encoding.UTF8;
        
        internal static BinaryWriter PutString(this BinaryWriter bw, string value)
        {
            var b = defaultEncoding.GetBytes(value);
            bw.Write((ushort)b.Length);
            if (b.Length > 0)
                bw.Write(b, 0, b.Length);
            return bw;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainMask(this string name) => name.Contains("*") || name.Contains("?");

        internal static void ValidatePageSize(int pageSize)
        {
            switch (pageSize)
            {
                case < PagedContainerHeader.MIN_PAGE_SIZE:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be >= {PagedContainerHeader.MIN_PAGE_SIZE} bytes (passed {pageSize} bytes)");
                case > 128 * 1024:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be <= 128 KB (passed {pageSize} bytes)");
            }
        }
    }
}