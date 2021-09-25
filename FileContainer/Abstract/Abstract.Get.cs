using System;
using System.Collections.Generic;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <exception cref="ArgumentException"></exception>
        public virtual byte[]? Get(string key) =>
            entries.TryGet(key, out var entry) && entry != null
                ? Stream.ReadEntryPageSequence(Header, entry)
                : null;

        /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
        public virtual Dictionary<string, byte[]> Get(params string[] keys)
        {
            var r = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);

            foreach (var entry in entries.Find(keys))
                r.Add(entry.Name, Stream.ReadEntryPageSequence(Header, entry));

            return r;
        }
    }
}