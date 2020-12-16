using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        readonly Dictionary<string, EntryReadonlyStream> attachedStreams = new Dictionary<string, EntryReadonlyStream>(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Create stream for reading entry with key
        /// Stream was attached ro PagedContainer and will detached at stream disposing
        /// Only one opened stream per key allowed
        /// 
        /// Value of entry can't be changed if existing opened stream for this key (exception occured at Append/Put/Delete operations) 
        /// </summary>
        [CanBeNull]
        public EntryReadonlyStream GetStream([NotNull] string key)
        {
            var entry = entries.Find(key).FirstOrDefault();
            if (entry == null)
                return null;

            if (attachedStreams.ContainsKey(entry.Name))
                throw new InvalidOperationException($"Stream {entry.Name} already attached & opened");

            var s = new EntryReadonlyStream(this, entry);
            attachedStreams.Add(entry.Name, s);
            return s;
        }

        void throwIfHasOpenedStream([NotNull] IEnumerable<string> keys)
        {
            foreach(var key in keys)
                if (attachedStreams.ContainsKey(key))
                    throw new InvalidOperationException($"Modify operation while attached ReadOnlyStream: {key}");
        }

        internal void DetachStream([NotNull] string key)
        {
            if (attachedStreams.TryGetValue(key, out var s))
                attachedStreams.Remove(key);
        }
    }
}