using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    /// <summary> Read only container, support of concurrent access to single container </summary>
    public class PersistentReadonlyContainer : PagedContainerAbstract
    {
        public PersistentReadonlyContainer([NotNull] string fileName, int pageSize = 4096) :
            base(File.Exists(fileName)
                     ? (Stream) new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, pageSize * 2)
                     : new MemoryStream(), pageSize)
        {
        }

        public override void     Put(string                        key, byte[] data) => throw new NotSupportedException();
        public override void     Append(string                     key, byte[] data) => throw new NotSupportedException();
        public override void     Append(Dictionary<string, byte[]> keyValues) => throw new NotSupportedException();
        public override string[] Delete(params string[]            keys)      => throw new NotSupportedException();
        public override void     Put(Dictionary<string, byte[]>    keyValues) => throw new NotSupportedException();
    }
}