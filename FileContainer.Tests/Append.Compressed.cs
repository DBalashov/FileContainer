using System;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestAppendsCompressed : TestAppends
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0) => 
            base.DoIt(action, PersistentContainerFlags.Compressed);

        public override void Append_Multi()  => Assert.Throws<NotSupportedException>(() => base.Append_Multi());
        public override void Append_Single() => Assert.Throws<NotSupportedException>(() => base.Append_Single());
    }
}