using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <exception cref="ArgumentException"></exception>
        [CanBeNull]
        public virtual byte[] Get([NotNull] string key) =>
            entries.TryGet(key, out var entry)
                ? Stream.ReadEntryPageSequence(Header, entry)
                : null;

        /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
        [NotNull]
        public virtual Dictionary<string, byte[]> Get(params string[] keys)
        {
            var r = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var entry in entries.Find(keys))
                r.Add(entry.Name, Stream.ReadEntryPageSequence(Header, entry));

            return r;
        }
    }
}