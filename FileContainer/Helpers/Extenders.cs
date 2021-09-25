using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace FileContainer
{
    static class Extenders
    {
        static readonly Encoding defaultEncoding = Encoding.UTF8;

        /// <summary>
        /// Read UTF-8 string from buff starting with offset
        /// string must be stored as [ushort length][utf-8 bytes]
        /// offset will forward
        /// </summary>
        internal static string GetString(this byte[] buff, ref int offset)
        {
            var nameLength = BitConverter.ToUInt16(buff, offset);
            offset += 2;

            if (nameLength <= 0)
                return "";

            var r = defaultEncoding.GetString(buff, offset, nameLength);
            offset += nameLength;
            return r;
        }


        internal static BinaryWriter PutString(this BinaryWriter bw, string value)
        {
            var b = defaultEncoding.GetBytes(value);
            bw.Write((ushort)b.Length);
            if (b.Length > 0)
                bw.Write(b, 0, b.Length);
            return bw;
        }

        /// <summary> read 32 bit int from buff starting with offset. Offset will forward to 4 bytes </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        internal static int GetInt(this byte[] buff, ref int offset)
        {
            var r = BitConverter.ToInt32(buff, offset);
            offset += 4;
            return r;
        }

        /// <summary> read 16 bit unsigned int from buff starting with offset. Offset will forward to 2 bytes </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        internal static ushort GetUInt16(this byte[] buff, ref int offset)
        {
            var r = BitConverter.ToUInt16(buff, offset);
            offset += 2;
            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainMask(this string name) => name.Contains("*") || name.Contains("?");

        internal static void ValidatePageSize(int pageSize)
        {
            switch (pageSize)
            {
                case < PagedContainerHeader.HEADER_PART:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be >= {PagedContainerHeader.HEADER_PART} bytes (passed {pageSize} bytes)");
                case > 128 * 1024:
                    throw new ArgumentException($"PagedContainerHeader: PageSize must be <= 128 KB (passed {pageSize} bytes)");
            }
        }
    }
}