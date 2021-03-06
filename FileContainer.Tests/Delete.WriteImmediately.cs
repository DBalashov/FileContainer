using System;

namespace FileContainer.Tests
{
    public class TestDeletesWriteImmediately : TestDeletes
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) => 
            base.DoIt(action, PersistentContainerFlags.WriteDirImmediately, 0);
    }
}