#region license

/*
Copyright (c) 2013-2017, Milosz Krajewski
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
using System.Runtime.CompilerServices;
using System.Text;

namespace LZ4
{
    public static class LZ4Codec
    {
        internal static readonly ILZ4Service Encoder;
        internal static readonly ILZ4Service Decoder;

        #region initialization

        /// <summary>Initializes the <see cref="LZ4Codec" /> class.</summary>
        static LZ4Codec()
        {
            var service32 = TryService<LZ4Service32>();
            var service64 = TryService<LZ4Service64>();

            // refer to: http://lz4net.codeplex.com/wikipage?title=Performance%20Testing for explanation about this order
            // feel free to change preferred order, just don't do it willy-nilly back it up with some evidence
            // it has been tested for Intel on Microsoft .NET only but looks reasonable for Mono as well
            if (IntPtr.Size == 4)
            {
                Encoder = service32 ?? service64;
                Decoder = service64 ?? service32;
            }
            else
            {
                Encoder = service32 ?? service64;
                Decoder = service64 ?? service32;
            }
        }

        /// <summary>Performs the quick auto-test on given compression service.</summary>
        /// <param name="service">The service.</param>
        /// <returns>A service or <c>null</c> if it failed.</returns>
        static ILZ4Service autoTest(ILZ4Service service)
        {
            const string loremIpsum = "Lorem ipsum dolor sit amet, consectetur adipisicing elit, sed do eiusmod tempor incididunt ut " +
                                      "labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco " +
                                      "laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in " +
                                      "voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat " +
                                      "non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

            int MaximumOutputLength(int inputLength) => inputLength + (inputLength / 255) + 16;
            
            // generate some well-known array of bytes
            const string inputText = loremIpsum + loremIpsum + loremIpsum + loremIpsum + loremIpsum;
            var          original  = Encoding.UTF8.GetBytes(inputText);

            // compress it
            var encoded       = new byte[MaximumOutputLength(original.Length)];
            var encodedLength = service.Encode(original, 0, original.Length, encoded, 0, encoded.Length);
            if (encodedLength < 0)
                return null;

            // decompress it (knowing original length)
            var decoded        = new byte[original.Length];
            var decodedLength1 = service.Decode(encoded, 0, encodedLength, decoded, 0, decoded.Length, true);
            if (decodedLength1 != original.Length)
                return null;
            var outputText1 = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            if (outputText1 != inputText)
                return null;

            // decompress it (not knowing original length)
            var decodedLength2 = service.Decode(encoded, 0, encodedLength, decoded, 0, decoded.Length, false);
            if (decodedLength2 != original.Length) return null;

            var outputText2 = Encoding.UTF8.GetString(decoded, 0, decoded.Length);
            return outputText2 != inputText ? null : service;
        }

        /// <summary>Tries to create a specified <seealso cref="ILZ4Service" /> and tests it.</summary>
        /// <typeparam name="T">Concrete <seealso cref="ILZ4Service" /> type.</typeparam>
        /// <returns>A service if succeeded or <c>null</c> if it failed.</returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        static ILZ4Service TryService<T>() where T : ILZ4Service, new()
        {
            try
            {
                return autoTest(new T());
            }
            catch (Exception)
            {
                // I could use Trace here but portable profile does not have Trace
                return null;
            }
        }

        #endregion

        #region Pack / Unpack

        const int WRAP_OFFSET_0 = 0;
        const int WRAP_OFFSET_4 = sizeof(int);
        const int WRAP_OFFSET_8 = 2 * sizeof(int);
        const int WRAP_LENGTH   = WRAP_OFFSET_8;

        /// <summary>Compresses and wraps given input byte buffer.</summary>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <param name="inputLength">Length of the input.</param>
        /// <returns>Compressed buffer.</returns>
        /// <exception cref="System.ArgumentException">inputBuffer size of inputLength is invalid</exception>
        public static byte[] Pack(byte[] inputBuffer, int inputOffset, int inputLength)
        {
            inputLength = Math.Min(inputBuffer.Length - inputOffset, inputLength);
            if (inputLength < 0)
                throw new ArgumentException("inputBuffer size of inputLength is invalid");
            if (inputLength == 0)
                return new byte[WRAP_LENGTH];

            var outputLength = inputLength; // MaximumOutputLength(inputLength);
            var outputBuffer = new byte[outputLength];

            outputLength = Encoder.Encode(inputBuffer, inputOffset, inputLength, outputBuffer, 0, outputLength);

            byte[] result;

            if (outputLength >= inputLength || outputLength <= 0)
            {
                result = new byte[inputLength + WRAP_LENGTH];
                poke4(result, WRAP_OFFSET_0, (uint)inputLength);
                poke4(result, WRAP_OFFSET_4, (uint)inputLength);
                Buffer.BlockCopy(inputBuffer, inputOffset, result, WRAP_OFFSET_8, inputLength);
            }
            else
            {
                result = new byte[outputLength + WRAP_LENGTH];
                poke4(result, WRAP_OFFSET_0, (uint)inputLength);
                poke4(result, WRAP_OFFSET_4, (uint)outputLength);
                Buffer.BlockCopy(outputBuffer, 0, result, WRAP_OFFSET_8, outputLength);
            }

            return result;
        }

        /// <summary>Unwraps the specified compressed buffer.</summary>
        /// <param name="inputBuffer">The input buffer.</param>
        /// <param name="inputOffset">The input offset.</param>
        /// <returns>Uncompressed buffer.</returns>
        /// <exception cref="System.ArgumentException">
        ///     inputBuffer size is invalid or inputBuffer size is invalid or has been corrupted
        /// </exception>
        public static byte[] Unpack(byte[] inputBuffer, int inputOffset = 0)
        {
            var inputLength = inputBuffer.Length - inputOffset;
            if (inputLength < WRAP_LENGTH)
                throw new ArgumentException("inputBuffer size is invalid");

            var outputLength = (int)LZ4ServiceBase.Peek4(inputBuffer, inputOffset + WRAP_OFFSET_0);
            inputLength = (int)LZ4ServiceBase.Peek4(inputBuffer, inputOffset + WRAP_OFFSET_4);
            if (inputLength > inputBuffer.Length - inputOffset - WRAP_LENGTH)
                throw new ArgumentException("inputBuffer size is invalid or has been corrupted");

            byte[] result;

            if (inputLength >= outputLength)
            {
                result = new byte[inputLength];
                Buffer.BlockCopy(inputBuffer, inputOffset + WRAP_OFFSET_8, result, 0, inputLength);
            }
            else
            {
                result = new byte[outputLength];
                Decoder.Decode(inputBuffer, inputOffset + WRAP_OFFSET_8, inputLength, result, 0, outputLength, true);
            }

            return result;
        }

        #endregion

        /// <summary>Sets uint32 value in byte buffer.</summary>
        /// <param name="buffer">The buffer.</param>
        /// <param name="offset">The offset.</param>
        /// <param name="value">The value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static void poke4(byte[] buffer, int offset, uint value)
        {
            buffer[offset + 0] = (byte)value;
            buffer[offset + 1] = (byte)(value >> 8);
            buffer[offset + 2] = (byte)(value >> 16);
            buffer[offset + 3] = (byte)(value >> 24);
        }
    }
}