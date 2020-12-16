using System;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

namespace FileContainer
{
    static class FilterExtenders
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static bool ContainMask([NotNull] this string name) => name.Contains('*') || name.Contains('?');
    }
}