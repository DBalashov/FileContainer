using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace FileContainer
{
    public static partial class ReadWriteExtenders
    {
        static readonly Encoding defaultEncoding = Encoding.UTF8;

        #region Get

        /// <exception cref="ArgumentException"></exception>
        [CanBeNull]
        public static string GetString([NotNull] this PagedContainerAbstract c, [NotNull] string key)
        {
            var value = c.Get(key);
            return value == null ? null : defaultEncoding.GetString(value);
        }

        /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
        [NotNull]
        public static Dictionary<string, string> GetString([NotNull] this PagedContainerAbstract c, params string[] keys) =>
            c.Get(keys).ToDictionary(p => p.Key, p => defaultEncoding.GetString(p.Value), StringComparer.InvariantCultureIgnoreCase);

        #endregion

        #region Put

        /// <summary> Create or replace entry with specified key </summary>
        /// <exception cref="ArgumentException"></exception>
        public static PutAppendResult Put([NotNull] this PagedContainerAbstract c, [NotNull] string key, [NotNull] string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Argument can't be null or empty", nameof(data));

            return c.Put(key, defaultEncoding.GetBytes(data));
        }

        /// <summary>
        /// Create or replace of passed entries.
        /// Value in dictionary must not be null or empty.
        /// </summary>
        /// <exception cref="ArgumentException"></exception>
        [NotNull]
        public static Dictionary<string, PutAppendResult> Put([NotNull] this PagedContainerAbstract c, [NotNull] Dictionary<string, string> keyValues)
        {
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (string.IsNullOrEmpty(item.Value))
                    throw new ArgumentException($"Argument can't be null or empty: {item.Key}", nameof(keyValues));
            }

            return c.Put(keyValues.ToDictionary(p => p.Key, p => defaultEncoding.GetBytes(p.Value), StringComparer.InvariantCultureIgnoreCase));
        }

        #endregion

        #region Append

        public static PutAppendResult Append([NotNull] this PagedContainerAbstract c, [NotNull] string key, [NotNull] string data)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Argument can't be null or empty", nameof(data));

            return c.Append(key, defaultEncoding.GetBytes(data));
        }

        [NotNull]
        public static Dictionary<string, PutAppendResult> Append([NotNull] this PagedContainerAbstract c, [NotNull] Dictionary<string, string> keyValues)
        {
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (string.IsNullOrEmpty(item.Value))
                    throw new ArgumentException($"Argument can't be null or empty: {item.Key}", nameof(keyValues));
            }

            return c.Append(keyValues.ToDictionary(p => p.Key, p => defaultEncoding.GetBytes(p.Value), StringComparer.InvariantCultureIgnoreCase));
        }

        #endregion
    }
}