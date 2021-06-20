using System.Runtime.CompilerServices;

namespace LZ4
{
    partial class LZ4Service64
    {
        #region LZ4_compressCtx

        static int LZ4_compressCtx_safe64(int[]  hash_table,
                                          byte[] src,
                                          byte[] dst,
                                          int    src_0,
                                          int    dst_0,
                                          int    src_len,
                                          int    dst_maxlen)
        {
            var debruijn64 = DEBRUIJN_TABLE_64;

            // ---- preprocessed source start here ----
            // r93
            var src_p       = src_0;
            var src_base    = src_0;
            var src_anchor  = src_p;
            var src_end     = src_p + src_len;
            var src_mflimit = src_end - MFLIMIT;

            var dst_p   = dst_0;
            var dst_end = dst_p + dst_maxlen;

            var src_LASTLITERALS   = src_end - LASTLITERALS;
            var src_LASTLITERALS_1 = src_LASTLITERALS - 1;

            var src_LASTLITERALS_3 = src_LASTLITERALS - 3;

            var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_64 - 1);
            var dst_LASTLITERALS_1          = dst_end - (1 + LASTLITERALS);
            var dst_LASTLITERALS_3          = dst_end - (2 + 1 + LASTLITERALS);

            // Init
            if (src_len < MINLENGTH) goto _last_literals;

            // First Byte
            hash_table[(((Peek4(src, src_p)) * 2654435761u) >> HASH_ADJUST)] = (src_p - src_base);
            src_p++;
            var h_fwd = (((Peek4(src, src_p)) * 2654435761u) >> HASH_ADJUST);

            // Main Loop
            while (true)
            {
                var  findMatchAttempts = (1 << SKIPSTRENGTH) + 3;
                var  src_p_fwd         = src_p;
                int  src_ref;
                uint h;
                
                // Find a match
                do
                {
                    h = h_fwd;
                    var step = findMatchAttempts++ >> SKIPSTRENGTH;
                    src_p     = src_p_fwd;
                    src_p_fwd = src_p + step;

                    if (src_p_fwd > src_mflimit) goto _last_literals;

                    h_fwd         = (((Peek4(src, src_p_fwd)) * 2654435761u) >> HASH_ADJUST);
                    src_ref       = src_base + hash_table[h];
                    hash_table[h] = (src_p - src_base);
                } while ((src_ref < src_p - MAX_DISTANCE) || (!Equal4(src, src_ref, src_p)));

                // Catch up
                while ((src_p > src_anchor) && (src_ref > src_0) && (src[src_p - 1] == src[src_ref - 1]))
                {
                    src_p--;
                    src_ref--;
                }

                // Encode Literal length
                var length    = (src_p - src_anchor);
                var dst_token = dst_p++;

                if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3) return 0; // Check output limit

                if (length >= RUN_MASK)
                {
                    var len = length - RUN_MASK;
                    dst[dst_token] = (RUN_MASK << ML_BITS);
                    if (len > 254)
                    {
                        do
                        {
                            dst[dst_p++] =  255;
                            len          -= 255;
                        } while (len > 254);

                        dst[dst_p++] = (byte)len;
                        blockCopy(src, src_anchor, dst, dst_p, length);
                        dst_p += length;
                        goto _next_match;
                    }
                    else
                        dst[dst_p++] = (byte)len;
                }
                else
                {
                    dst[dst_token] = (byte)(length << ML_BITS);
                }

                // Copy Literals
                if (length > 0)
                {
                    var _i = dst_p + length;
                    wildCopy(src, src_anchor, dst, dst_p, _i);
                    dst_p = _i;
                }

                _next_match:
                // Encode Offset
                Poke2(dst, dst_p, (ushort)(src_p - src_ref));
                dst_p += 2;

                // Start Counting
                src_p      += MINMATCH;
                src_ref    += MINMATCH; // MinMatch already verified
                src_anchor =  src_p;

                while (src_p < src_LASTLITERALS_STEPSIZE_1)
                {
                    var diff = (long)Xor8(src, src_ref, src_p);
                    if (diff == 0)
                    {
                        src_p   += STEPSIZE_64;
                        src_ref += STEPSIZE_64;
                        continue;
                    }

                    src_p += debruijn64[((ulong)((diff) & -(diff)) * 0x0218A392CDABBD3FL) >> 58];
                    goto _endCount;
                }

                if ((src_p < src_LASTLITERALS_3) && (Equal4(src, src_ref, src_p)))
                {
                    src_p   += 4;
                    src_ref += 4;
                }

                if ((src_p < src_LASTLITERALS_1) && (Equal2(src, src_ref, src_p)))
                {
                    src_p   += 2;
                    src_ref += 2;
                }

                if ((src_p < src_LASTLITERALS) && (src[src_ref] == src[src_p])) src_p++;

                _endCount:
                // Encode MatchLength
                length = (src_p - src_anchor);

                if (dst_p + (length >> 8) > dst_LASTLITERALS_1) return 0; // Check output limit

                if (length >= ML_MASK)
                {
                    dst[dst_token] += ML_MASK;
                    length         -= ML_MASK;
                    for (; length > 509; length -= 510)
                    {
                        dst[dst_p++] = 255;
                        dst[dst_p++] = 255;
                    }

                    if (length > 254)
                    {
                        length       -= 255;
                        dst[dst_p++] =  255;
                    }

                    dst[dst_p++] = (byte)length;
                }
                else
                {
                    dst[dst_token] += (byte)length;
                }

                // Test end of chunk
                if (src_p > src_mflimit)
                {
                    src_anchor = src_p;
                    break;
                }

                // Fill table
                hash_table[(((Peek4(src, src_p - 2)) * 2654435761u) >> HASH_ADJUST)] = (src_p - 2 - src_base);

                // Test next position

                h             = (((Peek4(src, src_p)) * 2654435761u) >> HASH_ADJUST);
                src_ref       = src_base + hash_table[h];
                hash_table[h] = (src_p - src_base);

                if ((src_ref > src_p - (MAX_DISTANCE + 1)) && (Equal4(src, src_ref, src_p)))
                {
                    dst_token      = dst_p++;
                    dst[dst_token] = 0;
                    goto _next_match;
                }

                // Prepare next loop
                src_anchor = src_p++;
                h_fwd      = (((Peek4(src, src_p)) * 2654435761u) >> HASH_ADJUST);
            }

            _last_literals:
            // Encode Last Literals
            {
                var lastRun = (src_end - src_anchor);

                if (dst_p + lastRun + 1 + ((lastRun + 255 - RUN_MASK) / 255) > dst_end) return 0;

                if (lastRun >= RUN_MASK)
                {
                    dst[dst_p++] =  (RUN_MASK << ML_BITS);
                    lastRun      -= RUN_MASK;
                    for (; lastRun > 254; lastRun -= 255) dst[dst_p++] = 255;
                    dst[dst_p++] = (byte)lastRun;
                }
                else dst[dst_p++] = (byte)(lastRun << ML_BITS);

                blockCopy(src, src_anchor, dst, dst_p, src_end - src_anchor);
                dst_p += src_end - src_anchor;
            }

            // End
            return (dst_p - dst_0);
        }

        #endregion

        #region LZ4_compress64kCtx

        static int LZ4_compress64kCtx_safe64(ushort[] hash_table,
                                             byte[]   src,
                                             byte[]   dst,
                                             int      src_0,
                                             int      dst_0,
                                             int      src_len,
                                             int      dst_maxlen)
        {
            var debruijn64 = DEBRUIJN_TABLE_64;
            int _i;

            // ---- preprocessed source start here ----
            // r93
            var src_p       = src_0;
            var src_anchor  = src_p;
            var src_base    = src_p;
            var src_end     = src_p + src_len;
            var src_mflimit = src_end - MFLIMIT;

            var dst_p   = dst_0;
            var dst_end = dst_p + dst_maxlen;

            var src_LASTLITERALS   = src_end - LASTLITERALS;
            var src_LASTLITERALS_1 = src_LASTLITERALS - 1;

            var src_LASTLITERALS_3 = src_LASTLITERALS - 3;

            var src_LASTLITERALS_STEPSIZE_1 = src_LASTLITERALS - (STEPSIZE_64 - 1);
            var dst_LASTLITERALS_1          = dst_end - (1 + LASTLITERALS);
            var dst_LASTLITERALS_3          = dst_end - (2 + 1 + LASTLITERALS);

            int len, length;

            uint h, h_fwd;

            // Init
            if (src_len < MINLENGTH) goto _last_literals;

            // First Byte
            src_p++;
            h_fwd = (((Peek4(src, src_p)) * 2654435761u) >> HASH64K_ADJUST);

            // Main Loop
            while (true)
            {
                var findMatchAttempts = (1 << SKIPSTRENGTH) + 3;
                var src_p_fwd         = src_p;
                int src_ref;
                int dst_token;

                // Find a match
                do
                {
                    h = h_fwd;
                    var step = findMatchAttempts++ >> SKIPSTRENGTH;
                    src_p     = src_p_fwd;
                    src_p_fwd = src_p + step;

                    if (src_p_fwd > src_mflimit) goto _last_literals;

                    h_fwd         = (((Peek4(src, src_p_fwd)) * 2654435761u) >> HASH64K_ADJUST);
                    src_ref       = src_base + hash_table[h];
                    hash_table[h] = (ushort)(src_p - src_base);
                } while (!Equal4(src, src_ref, src_p));

                // Catch up
                while ((src_p > src_anchor) && (src_ref > src_0) && (src[src_p - 1] == src[src_ref - 1]))
                {
                    src_p--;
                    src_ref--;
                }

                // Encode Literal length
                length    = (src_p - src_anchor);
                dst_token = dst_p++;

                if (dst_p + length + (length >> 8) > dst_LASTLITERALS_3) return 0; // Check output limit

                if (length >= RUN_MASK)
                {
                    len            = length - RUN_MASK;
                    dst[dst_token] = (RUN_MASK << ML_BITS);
                    if (len > 254)
                    {
                        do
                        {
                            dst[dst_p++] =  255;
                            len          -= 255;
                        } while (len > 254);

                        dst[dst_p++] = (byte)len;
                        blockCopy(src, src_anchor, dst, dst_p, length);
                        dst_p += length;
                        goto _next_match;
                    }
                    else
                        dst[dst_p++] = (byte)len;
                }
                else
                {
                    dst[dst_token] = (byte)(length << ML_BITS);
                }

                // Copy Literals
                if (length > 0) /*?*/
                {
                    _i = dst_p + length;
                    wildCopy(src, src_anchor, dst, dst_p, _i);
                    dst_p = _i;
                }

                _next_match:
                // Encode Offset
                Poke2(dst, dst_p, (ushort)(src_p - src_ref));
                dst_p += 2;

                // Start Counting
                src_p      += MINMATCH;
                src_ref    += MINMATCH; // MinMatch verified
                src_anchor =  src_p;

                while (src_p < src_LASTLITERALS_STEPSIZE_1)
                {
                    var diff = (long)Xor8(src, src_ref, src_p);
                    if (diff == 0)
                    {
                        src_p   += STEPSIZE_64;
                        src_ref += STEPSIZE_64;
                        continue;
                    }

                    src_p += debruijn64[((ulong)((diff) & -(diff)) * 0x0218A392CDABBD3FL) >> 58];
                    goto _endCount;
                }

                if ((src_p < src_LASTLITERALS_3) && (Equal4(src, src_ref, src_p)))
                {
                    src_p   += 4;
                    src_ref += 4;
                }

                if ((src_p < src_LASTLITERALS_1) && (Equal2(src, src_ref, src_p)))
                {
                    src_p   += 2;
                    src_ref += 2;
                }

                if ((src_p < src_LASTLITERALS) && (src[src_ref] == src[src_p])) src_p++;

                _endCount:

                // Encode MatchLength
                len = (src_p - src_anchor);

                if (dst_p + (len >> 8) > dst_LASTLITERALS_1) return 0; // Check output limit

                if (len >= ML_MASK)
                {
                    dst[dst_token] += ML_MASK;
                    len            -= ML_MASK;
                    for (; len > 509; len -= 510)
                    {
                        dst[dst_p++] = 255;
                        dst[dst_p++] = 255;
                    }

                    if (len > 254)
                    {
                        len          -= 255;
                        dst[dst_p++] =  255;
                    }

                    dst[dst_p++] = (byte)len;
                }
                else
                {
                    dst[dst_token] += (byte)len;
                }

                // Test end of chunk
                if (src_p > src_mflimit)
                {
                    src_anchor = src_p;
                    break;
                }

                // Fill table
                hash_table[(((Peek4(src, src_p - 2)) * 2654435761u) >> HASH64K_ADJUST)] = (ushort)(src_p - 2 - src_base);

                // Test next position

                h             = (((Peek4(src, src_p)) * 2654435761u) >> HASH64K_ADJUST);
                src_ref       = src_base + hash_table[h];
                hash_table[h] = (ushort)(src_p - src_base);

                if (Equal4(src, src_ref, src_p))
                {
                    dst_token      = dst_p++;
                    dst[dst_token] = 0;
                    goto _next_match;
                }

                // Prepare next loop
                src_anchor = src_p++;
                h_fwd      = (((Peek4(src, src_p)) * 2654435761u) >> HASH64K_ADJUST);
            }

            _last_literals:
            // Encode Last Literals
            {
                var lastRun = (src_end - src_anchor);
                if (dst_p + lastRun + 1 + (lastRun - RUN_MASK + 255) / 255 > dst_end) return 0;
                if (lastRun >= RUN_MASK)
                {
                    dst[dst_p++] =  (RUN_MASK << ML_BITS);
                    lastRun      -= RUN_MASK;
                    for (; lastRun > 254; lastRun -= 255) dst[dst_p++] = 255;
                    dst[dst_p++] = (byte)lastRun;
                }
                else dst[dst_p++] = (byte)(lastRun << ML_BITS);

                blockCopy(src, src_anchor, dst, dst_p, src_end - src_anchor);
                dst_p += src_end - src_anchor;
            }

            // End
            return (dst_p - dst_0);
        }

        #endregion

        #region LZ4_uncompress

        static int LZ4_uncompress_safe64(byte[] src,
                                         byte[] dst,
                                         int    src_0,
                                         int    dst_0,
                                         int    dst_len)
        {
            var dec32table = DECODER_TABLE_32;
            var dec64table = DECODER_TABLE_64;
            int _i;

            // ---- preprocessed source start here ----
            // r93
            var src_p = src_0;
            int dst_ref;

            var dst_p   = dst_0;
            var dst_end = dst_p + dst_len;
            int dst_cpy;

            var dst_LASTLITERALS          = dst_end - LASTLITERALS;
            var dst_COPYLENGTH            = dst_end - COPYLENGTH;
            var dst_COPYLENGTH_STEPSIZE_4 = dst_end - COPYLENGTH - (STEPSIZE_64 - 4);

            uint token;

            // Main Loop
            while (true)
            {
                int length;

                // get runlength
                token = src[src_p++];
                if ((length = (byte)(token >> ML_BITS)) == RUN_MASK)
                {
                    int len;
                    for (; (len = src[src_p++]) == 255; length += 255)
                    {
                        /* do nothing */
                    }

                    length += len;
                }

                // copy literals
                dst_cpy = dst_p + length;

                if (dst_cpy > dst_COPYLENGTH)
                {
                    if (dst_cpy != dst_end) goto _output_error; // Error : not enough place for another match (min 4) + 5 literals
                    blockCopy(src, src_p, dst, dst_p, length);
                    src_p += length;
                    break; // EOF
                }

                if (dst_p < dst_cpy) /*?*/
                {
                    _i    =  wildCopy(src, src_p, dst, dst_p, dst_cpy);
                    src_p += _i;
                    dst_p += _i;
                }

                src_p -= (dst_p - dst_cpy);
                dst_p =  dst_cpy;

                // get offset
                dst_ref =  (dst_cpy) - Peek2(src, src_p);
                src_p   += 2;
                if (dst_ref < dst_0) goto _output_error; // Error : offset outside destination buffer

                // get matchlength
                if ((length = (byte)(token & ML_MASK)) == ML_MASK)
                {
                    for (; src[src_p] == 255; length += 255) src_p++;
                    length += src[src_p++];
                }

                // copy repeated sequence
                if ((dst_p - dst_ref) < STEPSIZE_64)
                {
                    var dec64 = dec64table[dst_p - dst_ref];

                    dst[dst_p + 0] =  dst[dst_ref + 0];
                    dst[dst_p + 1] =  dst[dst_ref + 1];
                    dst[dst_p + 2] =  dst[dst_ref + 2];
                    dst[dst_p + 3] =  dst[dst_ref + 3];
                    dst_p          += 4;
                    dst_ref        += 4;
                    dst_ref        -= dec32table[dst_p - dst_ref];
                    copy4(dst, dst_ref, dst_p);
                    dst_p   += STEPSIZE_64 - 4;
                    dst_ref -= dec64;
                }
                else
                {
                    copy8(dst, dst_ref, dst_p);
                    dst_p   += 8;
                    dst_ref += 8;
                }

                dst_cpy = dst_p + length - (STEPSIZE_64 - 4);

                if (dst_cpy > dst_COPYLENGTH_STEPSIZE_4)
                {
                    if (dst_cpy > dst_LASTLITERALS) goto _output_error; // Error : last 5 bytes must be literals
                    if (dst_p < dst_COPYLENGTH)
                    {
                        _i      =  secureCopy(dst, dst_ref, dst_p, dst_COPYLENGTH);
                        dst_ref += _i;
                        dst_p   += _i;
                    }

                    while (dst_p < dst_cpy) dst[dst_p++] = dst[dst_ref++];
                    dst_p = dst_cpy;
                    continue;
                }

                if (dst_p < dst_cpy)
                {
                    secureCopy(dst, dst_ref, dst_p, dst_cpy);
                }

                dst_p = dst_cpy; // correction
            }

            // end of decoding
            return ((src_p) - src_0);

            _output_error:
            // write overflow error detected
            return (-((src_p) - src_0));
        }

        #endregion

        #region LZ4_uncompress_unknownOutputSize

        static int LZ4_uncompress_unknownOutputSize_safe64(byte[] src,
                                                           byte[] dst,
                                                           int    src_0,
                                                           int    dst_0,
                                                           int    src_len,
                                                           int    dst_maxlen)
        {
            var dec32table = DECODER_TABLE_32;
            var dec64table = DECODER_TABLE_64;
            int _i;

            // ---- preprocessed source start here ----
            // r93
            var src_p   = src_0;
            var src_end = src_p + src_len;
            int dst_ref;

            var dst_p   = dst_0;
            var dst_end = dst_p + dst_maxlen;
            int dst_cpy;

            var src_LASTLITERALS_3        = (src_end - (2 + 1 + LASTLITERALS));
            var src_LASTLITERALS_1        = (src_end - (LASTLITERALS + 1));
            var dst_COPYLENGTH            = (dst_end - COPYLENGTH);
            var dst_COPYLENGTH_STEPSIZE_4 = (dst_end - (COPYLENGTH + (STEPSIZE_64 - 4)));
            var dst_LASTLITERALS          = (dst_end - LASTLITERALS);
            var dst_MFLIMIT               = (dst_end - MFLIMIT);

            // Special case
            if (src_p == src_end) goto _output_error; // A correctly formed null-compressed LZ4 must have at least one byte (token=0)

            // Main Loop
            while (true)
            {
                byte token;
                int  length;

                // get runlength
                token = src[src_p++];
                if ((length = (token >> ML_BITS)) == RUN_MASK)
                {
                    var s                                          = 255;
                    while ((src_p < src_end) && (s == 255)) length += (s = src[src_p++]);
                }

                // copy literals
                dst_cpy = dst_p + length;

                if ((dst_cpy > dst_MFLIMIT) || (src_p + length > src_LASTLITERALS_3))
                {
                    if (dst_cpy > dst_end) goto _output_error; // Error : writes beyond output buffer
                    if (src_p + length != src_end)
                        goto
                            _output_error; // Error : LZ4 format requires to consume all input at this stage (no match within the last 11 bytes, and at least 8 remaining input bytes for another match+literals)
                    blockCopy(src, src_p, dst, dst_p, length);
                    dst_p += length;
                    break; // Necessarily EOF, due to parsing restrictions
                }

                if (dst_p < dst_cpy) /*?*/
                {
                    _i    =  wildCopy(src, src_p, dst, dst_p, dst_cpy);
                    src_p += _i;
                    dst_p += _i;
                }

                src_p -= (dst_p - dst_cpy);
                dst_p =  dst_cpy;

                // get offset
                dst_ref =  (dst_cpy) - Peek2(src, src_p);
                src_p   += 2;
                if (dst_ref < dst_0) goto _output_error; // Error : offset outside of destination buffer

                // get matchlength
                if ((length = (token & ML_MASK)) == ML_MASK)
                {
                    while (src_p < src_LASTLITERALS_1) // Error : a minimum input bytes must remain for LASTLITERALS + token
                    {
                        int s = src[src_p++];
                        length += s;
                        if (s == 255) continue;
                        break;
                    }
                }

                // copy repeated sequence
                if (dst_p - dst_ref < STEPSIZE_64)
                {
                    var dec64 = dec64table[dst_p - dst_ref];
                    dst[dst_p + 0] =  dst[dst_ref + 0];
                    dst[dst_p + 1] =  dst[dst_ref + 1];
                    dst[dst_p + 2] =  dst[dst_ref + 2];
                    dst[dst_p + 3] =  dst[dst_ref + 3];
                    dst_p          += 4;
                    dst_ref        += 4;
                    dst_ref        -= dec32table[dst_p - dst_ref];
                    copy4(dst, dst_ref, dst_p);
                    dst_p   += STEPSIZE_64 - 4;
                    dst_ref -= dec64;
                }
                else
                {
                    copy8(dst, dst_ref, dst_p);
                    dst_p   += 8;
                    dst_ref += 8;
                }

                dst_cpy = dst_p + length - (STEPSIZE_64 - 4);

                if (dst_cpy > dst_COPYLENGTH_STEPSIZE_4)
                {
                    if (dst_cpy > dst_LASTLITERALS) goto _output_error; // Error : last 5 bytes must be literals
                    if (dst_p < dst_COPYLENGTH)
                    {
                        _i      =  secureCopy(dst, dst_ref, dst_p, dst_COPYLENGTH);
                        dst_ref += _i;
                        dst_p   += _i;
                    }

                    while (dst_p < dst_cpy) dst[dst_p++] = dst[dst_ref++];
                    dst_p = dst_cpy;
                    continue;
                }

                if (dst_p < dst_cpy)
                {
                    secureCopy(dst, dst_ref, dst_p, dst_cpy);
                }

                dst_p = dst_cpy; // correction
            }

            // end of decoding
            return ((dst_p) - dst_0);

            // write overflow error detected
            _output_error:
            return (-((src_p) - src_0));
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void copy8(byte[] buf, int src, int dst)
        {
            buf[dst + 7] = buf[src + 7];
            buf[dst + 6] = buf[src + 6];
            buf[dst + 5] = buf[src + 5];
            buf[dst + 4] = buf[src + 4];
            buf[dst + 3] = buf[src + 3];
            buf[dst + 2] = buf[src + 2];
            buf[dst + 1] = buf[src + 1];
            buf[dst]     = buf[src];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static ulong Xor8(byte[] buffer, int offset1, int offset2)
        {
            var value1 = ((ulong)buffer[offset1]) |
                         ((ulong)buffer[offset1 + 1] << 8) |
                         ((ulong)buffer[offset1 + 2] << 16) |
                         ((ulong)buffer[offset1 + 3] << 24) |
                         ((ulong)buffer[offset1 + 4] << 32) |
                         ((ulong)buffer[offset1 + 5] << 40) |
                         ((ulong)buffer[offset1 + 6] << 48) |
                         ((ulong)buffer[offset1 + 7] << 56);
            var value2 = ((ulong)buffer[offset2]) |
                         ((ulong)buffer[offset2 + 1] << 8) |
                         ((ulong)buffer[offset2 + 2] << 16) |
                         ((ulong)buffer[offset2 + 3] << 24) |
                         ((ulong)buffer[offset2 + 4] << 32) |
                         ((ulong)buffer[offset2 + 5] << 40) |
                         ((ulong)buffer[offset2 + 6] << 48) |
                         ((ulong)buffer[offset2 + 7] << 56);
            return value1 ^ value2;
        }
    }
}