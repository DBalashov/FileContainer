using System;

namespace FileContainer.Tests
{
    public class TestReadsCompressed_GZip : TestReads
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, flags, PersistentContainerCompressType.GZip);
    }
    
    public class TestReadsCompressed_LZ4 : TestReads
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, flags, PersistentContainerCompressType.LZ4);
    }
}