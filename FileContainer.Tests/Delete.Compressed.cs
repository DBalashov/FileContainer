using System;

namespace FileContainer.Tests
{
    public class TestDeletesCompressed_GZip : TestDeletes
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, PersistentContainerFlags.Compressed, PersistentContainerCompressType.GZip);
    }
    
    public class TestDeletesCompressed_LZ4 : TestDeletes
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, PersistentContainerFlags.Compressed, PersistentContainerCompressType.LZ4);
    }
}