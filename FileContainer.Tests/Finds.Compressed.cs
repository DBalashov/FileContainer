using System;

namespace FileContainer.Tests
{
    public class TestFindsCompressed : TestFinds
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0) => 
            base.DoIt(action, PersistentContainerFlags.Compressed);
    }
}