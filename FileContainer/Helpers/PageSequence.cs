using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;

namespace FileContainer
{
    readonly struct PageSequence
    {
        [NotNull] internal readonly byte[] Data;
        [NotNull] internal readonly int[]  Pages;

        internal PageSequence([NotNull] byte[] data, [NotNull] int[] pages)
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