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
            if (!externalMemoryStream) // если стрим был создан в KVInMemoryStore - убиваем его вместе со всем остальным, иначе не трогаем т.к. он пришёл снаружи
                base.DisposeStream();
        }
    }
}