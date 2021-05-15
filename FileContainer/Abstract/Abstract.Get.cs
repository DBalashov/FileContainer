using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        #region byte[]

        /// <exception cref="ArgumentException"></exception>
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

        #endregion

        #region string

        /// <exception cref="ArgumentException"></exception>
        [CanBeNull]
        public virtual string GetString([NotNull] string key)
        {
            var value = Get(key);
            return value == null ? null : defaultEncoding.GetString(value);
        }

        /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
        [NotNull]
        public virtual Dictionary<string, string> GetString(params string[] keys) =>
            Get(keys).ToDictionary(p => p.Key, p => defaultEncoding.GetString(p.Value), StringComparer.InvariantCultureIgnoreCase);

        #endregion
    }
}