using System;
using System.Runtime.CompilerServices;

namespace FileContainer;

static class Extenders
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static bool ContainMask(this string name) => name.Contains('*') || name.Contains('?');

    internal static void ValidatePageSize(int pageSize)
    {
        switch (pageSize)
        {
            case < PagedContainerHeader.MIN_PAGE_SIZE:
                throw new ArgumentException($"PagedContainerHeader: PageSize must be >= {PagedContainerHeader.MIN_PAGE_SIZE} bytes (passed {pageSize} bytes)");
            case > 128 * 1024:
                throw new ArgumentException($"PagedContainerHeader: PageSize must be <= 128 KB (passed {pageSize} bytes)");
        }
    }
}