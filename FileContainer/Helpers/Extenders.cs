using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;

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
        [NotNull]
        internal static string GetString([NotNull] this byte[] buff, ref int offset)
        {
            var nameLength = BitConverter.ToUInt16(buff, offset);
            offset += 2;

            if (nameLength > 0)
            {
                var r = defaultEncoding.GetString(buff, offset, nameLength);
                offset += nameLength;
                return r;
            }

            return "";
        }

        [NotNull]
        internal static BinaryWriter PutString([NotNull] this BinaryWriter bw, string value)
        {
            var b = defaultEncoding.GetBytes(value);
            bw.Write((ushort) b.Length);
            if (b.Length > 0)
                bw.Write(b, 0, b.Length);
            return bw;
        }

        /// <summary> read 32 bit int from buff starting with offset. Offset will forward to 4 bytes </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [DebuggerStepThrough]
        internal static int GetInt([NotNull] this byte[] buff, ref int offset)
        {
            var r = BitConverter.ToInt32(buff, offset);
            offset += 4;
            return r;
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainMask([NotNull] this string name) => name.Contains("*") || name.Contains("?");
    }
}