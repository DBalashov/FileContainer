using Microsoft.IO;

namespace FileContainer;

static class MemoryStreamManager
{
    internal static readonly RecyclableMemoryStreamManager Instance = new();
}