using System;
using NUnit.Framework;

namespace FileContainer.Tests
{
    public class TestAppendsCompressed_GZip : TestAppends
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) =>
            base.DoIt(action, flags, PersistentContainerCompressType.GZip);

        public override void Append_Multi()  => Assert.Throws<NotSupportedException>(() => base.Append_Multi());
        public override void Append_Single() => Assert.Throws<NotSupportedException>(() => base.Append_Single());
    }

    public class TestAppendsCompressed_LZ4 : TestAppends
    {
        protected override void DoIt(Action<Func<PagedContainerAbstract>> action, PersistentContainerFlags flags = 0, PersistentContainerCompressType compressType = 0) =>
            base.DoIt(action, flags, PersistentContainerCompressType.LZ4);

        public override void Append_Multi()  => Assert.Throws<NotSupportedException>(() => base.Append_Multi());
        public override void Append_Single() => Assert.Throws<NotSupportedException>(() => base.Append_Single());
    }
}