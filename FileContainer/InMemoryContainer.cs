using System;
using System.IO;

namespace FileContainer
{
    public class InMemoryContainer : PagedContainerAbstract
    {
        readonly bool externalMemoryStream = false;

        public InMemoryContainer(PersistentContainerSettings? settings = null) : base(new MemoryStream(), settings)
        {
        }

        public InMemoryContainer(MemoryStream stm, PersistentContainerSettings? settings = null) : base(stm, settings) =>
            externalMemoryStream = true;

        protected override void DisposeStream()
        {
            if (!externalMemoryStream) // if the own stream - disposing, otherwise no action due to the external stream
                base.DisposeStream();
        }
    }
}