using System;
using System.Runtime.CompilerServices;

namespace LZ4
{
    abstract class LZ4ServiceBase
    {
        #region consts

        /// <summary>
        /// Memory usage formula : N->2^N Bytes (examples : 10 -> 1KB; 12 -> 4KB ; 16 -> 64KB; 20 -> 1MB; etc.)
        /// Increasing memory usage improves compression ratio
        /// Reduced memory usage can improve speed, due to cache effect
        /// Default value is 14, for 16KB, which nicely fits into Intel x86 L1 cache
        /// </summary>
        protected const int MEMORY_USAGE = 14;

        protected const int COPYLENGTH   = 8;
        protected const int MINMATCH     = 4;
        protected const int MFLIMIT      = COPYLENGTH + MINMATCH;
        protected const int LZ4_64KLIMIT = (1 << 16) + (MFLIMIT - 1);

        protected const int HASH_LOG       = MEMORY_USAGE - 2;
        protected const int HASH_TABLESIZE = 1 << HASH_LOG;
        protected const int HASH_ADJUST    = (MINMATCH * 8) - HASH_LOG;

        protected const int HASH64K_LOG       = HASH_LOG + 1;
        protected const int HASH64K_TABLESIZE = 1 << HASH64K_LOG;
        protected const int HASH64K_ADJUST    = (MINMATCH * 8) - HASH64K_LOG;

        protected const int LASTLITERALS = 5;

        protected const int MINLENGTH = MFLIMIT + 1;

        protected const int MAXD_LOG     = 16;
        // protected const int MAXD         = 1 << MAXD_LOG;
        // protected const int MAXD_MASK    = MAXD - 1;
        protected const int MAX_DISTANCE = (1 << MAXD_LOG) - 1;
        protected const int ML_BITS      = 4;
        protected const int ML_MASK      = (1 << ML_BITS) - 1;
        protected const int RUN_BITS     = 8 - ML_BITS;
        protected const int RUN_MASK     = (1 << RUN_BITS) - 1;
        protected const int STEPSIZE_64  = 8;

        /// <summary>Buffer length when Buffer.BlockCopy becomes faster than straight loop.
        /// Please note that safe implementation REQUIRES it to be greater (not even equal) than 8.</summary>
        const int BLOCK_COPY_LIMIT = 16;

        /// <summary>
        /// Decreasing this value will make the algorithm skip faster data segments considered "incompressible"
        /// This may decrease compression ratio dramatically, but will be faster on incompressible data
        /// Increasing this value will make the algorithm search more before declaring a segment "incompressible"
        /// This could improve compression a bit, but will be slower on incompressible data
        /// The default value (6) is recommended
        /// </summary>
        protected const int NOTCOMPRESSIBLE_DETECTIONLEVEL = 6;

        protected const int SKIPSTRENGTH = NOTCOMPRESSIBLE_DETECTIONLEVEL > 2 ? NOTCOMPRESSIBLE_DETECTIONLEVEL : 2;

        protected static readonly int[] DECODER_TABLE_32 = { 0, 3, 2, 3, 0, 0, 0, 0 };

        #endregion

        #region internal interface (common)

        protected static void checkArguments(byte[] input,  int inputOffset,  ref int inputLength,
                                             byte[] output, int outputOffset, ref int outputLength)
        {
            if (inputLength < 0) inputLength = input.Length - inputOffset;
            if (inputLength == 0)
            {
                outputLength = 0;
                return;
            }

            if (input == null) throw new ArgumentNullException(nameof(input));
            if (inputOffset < 0 || inputOffset + inputLength > input.Length)
                throw new ArgumentException("inputOffset and inputLength are invalid for given input");

            if (outputLength < 0) outputLength = output.Length - outputOffset;
            if (output == null) throw new ArgumentNullException(nameof(output));
            if (outputOffset < 0 || outputOffset + outputLength > output.Length)
                throw new ArgumentException("outputOffset and outputLength are invalid for given output");
        }

        #endregion

        /// <summary>Gets uint32 from byte buffer.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <returns>The value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static uint Peek4(byte[] buffer, int offset) => ((uint)buffer[offset]) |
                                                                 ((uint)buffer[offset + 1] << 8) |
                                                                 ((uint)buffer[offset + 2] << 16) |
                                                                 ((uint)buffer[offset + 3] << 24);

        #region Byte manipulation

        // ReSharper disable RedundantCast
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void Poke2(byte[] buffer, int offset, ushort value)
        {
            buffer[offset]     = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static ushort Peek2(byte[] buffer, int offset) =>
            (ushort)(((uint)buffer[offset]) | ((uint)buffer[offset + 1] << 8));
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool Equal2(byte[] buffer, int offset1, int offset2) =>
            buffer[offset1] == buffer[offset2] && buffer[offset1 + 1] == buffer[offset2 + 1];

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool Equal4(byte[] buffer, int offset1, int offset2)
        {
            if (buffer[offset1] != buffer[offset2]) return false;
            if (buffer[offset1 + 1] != buffer[offset2 + 1]) return false;
            if (buffer[offset1 + 2] != buffer[offset2 + 2]) return false;
            return buffer[offset1 + 3] == buffer[offset2 + 3];
        }

        // ReSharper restore RedundantCast

        #endregion

        #region Byte block copy

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void copy4(byte[] buf, int src, int dst)
        {
            buf[dst + 3] = buf[src + 3];
            buf[dst + 2] = buf[src + 2];
            buf[dst + 1] = buf[src + 1];
            buf[dst]     = buf[src];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void blockCopy(byte[] src, int src_0, byte[] dst, int dst_0, int len)
        {
            if (len >= BLOCK_COPY_LIMIT)
            {
                Buffer.BlockCopy(src, src_0, dst, dst_0, len);
            }
            else
            {
                while (len >= 8)
                {
                    dst[dst_0]     =  src[src_0];
                    dst[dst_0 + 1] =  src[src_0 + 1];
                    dst[dst_0 + 2] =  src[src_0 + 2];
                    dst[dst_0 + 3] =  src[src_0 + 3];
                    dst[dst_0 + 4] =  src[src_0 + 4];
                    dst[dst_0 + 5] =  src[src_0 + 5];
                    dst[dst_0 + 6] =  src[src_0 + 6];
                    dst[dst_0 + 7] =  src[src_0 + 7];
                    len            -= 8;
                    src_0          += 8;
                    dst_0          += 8;
                }

                while (len >= 4)
                {
                    dst[dst_0]     =  src[src_0];
                    dst[dst_0 + 1] =  src[src_0 + 1];
                    dst[dst_0 + 2] =  src[src_0 + 2];
                    dst[dst_0 + 3] =  src[src_0 + 3];
                    len            -= 4;
                    src_0          += 4;
                    dst_0          += 4;
                }

                while (len-- > 0)
                {
                    dst[dst_0++] = src[src_0++];
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int wildCopy(byte[] src, int src_0, byte[] dst, int dst_0, int dst_end)
        {
            var len = dst_end - dst_0;

            if (len >= BLOCK_COPY_LIMIT)
            {
                Buffer.BlockCopy(src, src_0, dst, dst_0, len);
            }
            else
            {
                while (len >= 4)
                {
                    dst[dst_0]     =  src[src_0];
                    dst[dst_0 + 1] =  src[src_0 + 1];
                    dst[dst_0 + 2] =  src[src_0 + 2];
                    dst[dst_0 + 3] =  src[src_0 + 3];
                    len            -= 4;
                    src_0          += 4;
                    dst_0          += 4;
                }

                while (len-- > 0)
                {
                    dst[dst_0++] = src[src_0++];
                }
            }

            return len;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static int secureCopy(byte[] buffer, int src, int dst, int dst_end)
        {
            var diff   = dst - src;
            var length = dst_end - dst;
            var len    = length;

            if (diff >= BLOCK_COPY_LIMIT)
            {
                if (diff >= length)
                {
                    Buffer.BlockCopy(buffer, src, buffer, dst, length);
                    return length; // done
                }

                do
                {
                    Buffer.BlockCopy(buffer, src, buffer, dst, diff);
                    src += diff;
                    dst += diff;
                    len -= diff;
                } while (len >= diff);
            }

            while (len >= 4)
            {
                buffer[dst]     =  buffer[src];
                buffer[dst + 1] =  buffer[src + 1];
                buffer[dst + 2] =  buffer[src + 2];
                buffer[dst + 3] =  buffer[src + 3];
                dst             += 4;
                src             += 4;
                len             -= 4;
            }

            while (len-- > 0)
            {
                buffer[dst++] = buffer[src++];
            }

            return length; // done
        }

        #endregion
    }
}