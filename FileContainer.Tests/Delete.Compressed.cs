using System;

namespace FileContainer.Tests
{
    public class TestDeletesCompressed : TestDeletes
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0) => 
            base.DoIt(action, PersistentContainerFlags.Compressed);
    }
}