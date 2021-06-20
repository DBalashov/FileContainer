using System;

namespace FileContainer.Tests
{
    public class TestReadsWriteImmediately : TestReads
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0) => 
            base.DoIt(action, PersistentContainerFlags.WriteDirImmediately);
    }
}