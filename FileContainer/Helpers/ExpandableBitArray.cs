using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace FileContainer
{
    public sealed class ExpandableBitArray
    {
        const int  BPV     = sizeof(uint) * 8;
        const uint ALL_ON  = 0xFFFFFFFF;
        const uint ALL_OFF = 0;

        uint[] values;

        public int Length => values.Length * BPV;

        public ExpandableBitArray(int minimumBits)
        {
            if (minimumBits < 1)
                throw new ArgumentOutOfRangeException(nameof(minimumBits), minimumBits, "parameter must be >=1");

            values = new uint[bitsToValues(minimumBits)];
        }

        /// <summary> create bit array from bytes. If bytes unaligned to 4 bytes (32 bits) - array will expanded to nearest 4 byte length with zero bits </summary>
        public ExpandableBitArray(byte[] bytes)
        {
            var byteInValue = BPV / 8;
            if (bytes.Length % byteInValue != 0)
                Array.Resize(ref bytes, (bytes.Length / byteInValue + 1) * byteInValue);

            values = new uint[bytes.Length / byteInValue];
            Buffer.BlockCopy(bytes, 0, values, 0, values.Length * byteInValue);
        }

        /// <summary> Expand bit array to specified bits with zero bits. Bits will ceiling to 32 bits. </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ResizeTo(int bits) =>
            Array.Resize(ref values, bitsToValues(bits));

        /// <summary>
        /// Set or Get bit with specified index.
        /// If Set called and index of bit > Length - bit array will expanded 
        /// </summary>
        public bool this[int index]
        {
            get
            {
                if (index >= Length) return false;
                return (values[index / BPV] & (1 << (index % BPV))) != 0;
            }
            set
            {
                if (index >= Length)
                    ResizeTo(index + 1);

                setBit(index, value);
            }
        }

        /// <summary>
        /// Set 'count' bits from startFromBitIndex to passed state.
        /// If startFromBitIndex + count > Length - bits array will expanded 
        /// </summary>
        [ExcludeFromCodeCoverage] // not used now
        public void Set(int startFromBitIndex, int count, bool state = true)
        {
            if (startFromBitIndex + count >= Length)
                ResizeTo(startFromBitIndex + count);

            while (count > 0)
            {
                setBit(startFromBitIndex, state);

                count--;
                startFromBitIndex++;
            }
        }

        /// <summary>
        /// Return bit indexes from 0-index with state. Can be used with Skip/Take for make fixed bits array.
        /// Bits array not expanded.
        /// </summary>
        public IEnumerable<int> GetBits(bool withState)
        {
            int bitIndex = 0;
            foreach (var value in values)
            {
                if (withState)
                {
                    if (value == ALL_OFF)
                    {
                        bitIndex += BPV;
                        continue;
                    }
                }
                else
                {
                    if (value == ALL_ON)
                    {
                        bitIndex += BPV;
                        continue;
                    }
                }

                for (var i = 0; i < BPV; i++)
                {
                    var maskedValue = value & (1 << i);
                    if (withState)
                    {
                        if (maskedValue != 0) yield return bitIndex;
                    }
                    else
                    {
                        if (maskedValue == 0) yield return bitIndex;
                    }

                    bitIndex++;
                }
            }
        }

        /// <summary> Set bits with passed indexes to state. if bitIndex > Length - bits array will expanded </summary>
        [ExcludeFromCodeCoverage] // not used now
        public void SetBits(int[] bitIndexes, bool toState = false)
        {
            foreach (var bitIndex in bitIndexes)
            {
                if (bitIndex >= Length)
                    ResizeTo(bitIndex);

                setBit(bitIndex, toState);
            }
        }

        /// <summary> return internal representation of bits. Can be used in constructor for clone (for example) </summary>
        public byte[] GetBytes() => 
            MemoryMarshal.Cast<uint, byte>(values).ToArray();

        #region privates

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void setBit(int bitIndex, bool toState)
        {
            var valueIndex = bitIndex / BPV;
            var mask       = 1 << (bitIndex % BPV);
            if (toState)
                values[valueIndex]  |= (uint)mask;
            else values[valueIndex] &= (uint)~mask;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int bitsToValues(int bits) =>
            bits / BPV + (bits % BPV == 0 ? 0 : 1);

        #endregion

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("Bits: {0}\n", values.Length * BPV);

            int counter = 0;
            foreach (var value in values)
            {
                if (counter > 0)
                    sb.AppendLine();

                sb.Append(counter.ToString("D3"));
                sb.Append(":");

                for (var k = 0; k < BPV; k++)
                {
                    if (k % 8 == 0) sb.Append(' ');
                    sb.Append((value & (1 << k)) > 0 ? "1" : ".");
                }

                counter++;
            }

            return sb.ToString();
        }
#endif
    }
}