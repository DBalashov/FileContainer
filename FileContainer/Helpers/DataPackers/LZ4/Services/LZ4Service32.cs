#region license

/*
Copyright (c) 2013, Milosz Krajewski
All rights reserved.

Redistribution and use in source and binary forms, with or without modification, are permitted provided 
that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this list of conditions 
  and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice, this list of conditions 
  and the following disclaimer in the documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED 
WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR 
A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE 
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT 
LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, 
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN 
IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

#endregion

using System;
using System.Diagnostics.CodeAnalysis;

namespace LZ4
{
    [ExcludeFromCodeCoverage]
    partial class LZ4Service32 : LZ4ServiceBase, ILZ4Service
    {
        const int STEPSIZE_32 = 4;
        
        protected static readonly int[] DEBRUIJN_TABLE_32 =
        {
            0, 0, 3, 0, 3, 1, 3, 0, 3, 2, 2, 1, 3, 2, 0, 1,
            3, 3, 1, 2, 2, 2, 2, 0, 3, 1, 2, 0, 1, 0, 1, 1
        };
        
        /// <summary>Encodes the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputLength">Length of the input.</param>
        /// <param name="output">The output.</param>
        /// <param name="outputOffset">The output offset.</param>
        /// <param name="outputLength">Length of the output.</param>
        /// <returns>Number of bytes written.</returns>
        public int Encode(byte[] input,
                          int    inputOffset,
                          int    inputLength,
                          byte[] output,
                          int    outputOffset,
                          int    outputLength)
        {
            checkArguments(input, inputOffset, ref inputLength,
                           output, outputOffset, ref outputLength);
            if (outputLength == 0) return 0;

            if (inputLength < LZ4_64KLIMIT)
            {
                var hashTable = new ushort[HASH64K_TABLESIZE];
                return LZ4_compress64kCtx_safe32(hashTable, input, output, inputOffset, outputOffset, inputLength, outputLength);
            }
            else
            {
                var hashTable = new int[HASH_TABLESIZE];
                return LZ4_compressCtx_safe32(hashTable, input, output, inputOffset, outputOffset, inputLength, outputLength);
            }
        }

        /// <summary>Decodes the specified input.</summary>
        /// <param name="input">The input.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputLength">Length of the input.</param>
        /// <param name="output">The output.</param>
        /// <param name="outputOffset">The output offset.</param>
        /// <param name="outputLength">Length of the output.</param>
        /// <param name="knownOutputLength">Set it to <c>true</c> if output length is known.</param>
        /// <returns>Number of bytes written.</returns>
        public int Decode(byte[] input,
                          int    inputOffset,
                          int    inputLength,
                          byte[] output,
                          int    outputOffset,
                          int    outputLength,
                          bool   knownOutputLength)
        {
            checkArguments(input, inputOffset, ref inputLength,
                           output, outputOffset, ref outputLength);

            if (outputLength == 0) return 0;

            if (knownOutputLength)
            {
                var length = LZ4_uncompress_safe32(input, output, inputOffset, outputOffset, outputLength);
                if (length != inputLength)
                    throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
                return outputLength;
            }
            else
            {
                var length = LZ4_uncompress_unknownOutputSize_safe32(input, output, inputOffset, outputOffset, inputLength, outputLength);
                if (length < 0)
                    throw new ArgumentException("LZ4 block is corrupted, or invalid length has been given.");
                return length;
            }
        }


    }
}