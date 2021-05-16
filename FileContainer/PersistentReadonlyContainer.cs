using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    /// <summary> Read only container, support of concurrent access to single container </summary>
    public class PersistentReadonlyContainer : PagedContainerAbstract
    {
        public PersistentReadonlyContainer([NotNull] string fileName, PersistentContainerSettings settings = null) :
            base(File.Exists(fileName)
                     ? new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, (settings?.PageSize ?? 4096) * 2)
                     : new MemoryStream(), settings)
        {
        }

        public override PutAppendResult                     Put(string                     key, byte[] data) => throw new NotSupportedException();
        public override Dictionary<string, PutAppendResult> Put(Dictionary<string, byte[]> keyValues) => throw new NotSupportedException();

        public override PutAppendResult                     Append(string                     key, byte[] data) => throw new NotSupportedException();
        public override Dictionary<string, PutAppendResult> Append(Dictionary<string, byte[]> keyValues) => throw new NotSupportedException();
        public override string[]                            Delete(params string[]            keys)      => throw new NotSupportedException();
    }
}