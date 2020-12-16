using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        [CanBeNull]
        public virtual byte[] Get([NotNull] string key) =>
            entries.TryGet(key, out var entry)
                ? stm.ReadEntryPageSequence(header, entry)
                : null;

        /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
        [NotNull]
        public virtual Dictionary<string, byte[]> Get(params string[] keys)
        {
            var r = new Dictionary<string, byte[]>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var entry in entries.Find(keys))
                r.Add(entry.Name, stm.ReadEntryPageSequence(header, entry));

            return r;
        }

        // [CanBeNull]
        // public virtual byte[] Get([NotNull] string key, int offset, int? length = null)
        // {
        //     throw new NotImplementedException("KVAbstractStore.Get");
        // }
        //
        // [CanBeNull]
        // public virtual bool Get([NotNull] string key,         byte[] buff, int buffOffset,
        //                         int              entryOffset, int?   length = null)
        // {
        //     throw new NotImplementedException("KVAbstractStore.Get");
        // }
    }
}