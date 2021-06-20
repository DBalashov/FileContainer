using System;

namespace FileContainer.Tests
{
    public class TestAppendsWriteImmediately : TestAppends
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = (PersistentContainerFlags)0)
        {
            base.DoIt(action, PersistentContainerFlags.WriteDirImmediately);
        }
    }
}