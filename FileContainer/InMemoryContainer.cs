using System;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    public class InMemoryContainer : PagedContainerAbstract
    {
        readonly bool externalMemoryStream = false;

        public InMemoryContainer(int pageSize = 4096) : base(new MemoryStream(), pageSize)
        {
        }

        public InMemoryContainer([NotNull] MemoryStream stm, int pageSize = 4096) : base(stm, pageSize) =>
            externalMemoryStream = true;

        protected override void DisposeStream()
        {
            if (!externalMemoryStream) // if the own stream - disposing, otherwise no action due to the external stream
                base.DisposeStream();
        }
    }
}