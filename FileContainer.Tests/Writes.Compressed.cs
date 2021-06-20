using System;

namespace FileContainer.Tests
{
    public class TestWritesCompressed_GZip : TestWrites
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) =>
            base.DoIt(action, PersistentContainerFlags.Compressed, PersistentContainerCompressType.GZip);
    }

    public class TestWritesCompressed_LZ4 : TestWrites
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) =>
            base.DoIt(action, PersistentContainerFlags.Compressed, PersistentContainerCompressType.LZ4);
    }
}