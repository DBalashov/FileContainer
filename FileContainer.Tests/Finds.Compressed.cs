using System;

namespace FileContainer.Tests
{
    public class TestFindsCompressed_GZip : TestFinds
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, flags, PersistentContainerCompressType.GZip);
    }
    
    public class TestFindsCompressed : TestFinds
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, flags, PersistentContainerCompressType.LZ4);
    }
}