using System.Diagnostics.CodeAnalysis;

namespace FileContainer
{
    readonly struct PageSequence
    {
        internal readonly byte[] Data;
        internal readonly int[]  Pages;

        internal PageSequence(byte[] data, int[] pages)
        {
            Data  = data;
            Pages = pages;
        }

#if DEBUG
        [ExcludeFromCodeCoverage]
        public override string ToString() => $"{Data.Length} bytes, {Pages.Length} pages";
#endif
    }
}