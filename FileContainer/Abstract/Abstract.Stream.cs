using System;
using System.Collections.Generic;
using System.Linq;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        readonly Dictionary<string, EntryReadonlyStream> attachedStreams = new(StringComparer.InvariantCultureIgnoreCase);

        /// <summary>
        /// Create stream for reading entry with key.
        /// Stream was attached to PagedContainer and will detached at stream disposing.
        /// Only one opened stream per key allowed.
        /// Mask allowed, but only first founded item will opened. 
        /// 
        /// Value of entry can't be changed and exception will throwed (Append/Delete/Put operations) until existing stream was closed. 
        /// </summary>
        public EntryReadonlyStream? GetStream(string key)
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

        /// <summary> throw if opened stream of any key found </summary>
        /// <exception cref="InvalidOperationException"></exception>
        void throwIfHasOpenedStream(IEnumerable<string> keys)
        {
            foreach (var key in keys)
                if (attachedStreams.ContainsKey(key))
                    throw new InvalidOperationException($"Modify operation while attached ReadOnlyStream: {key}");
        }

        internal void DetachStream(string key)
        {
            if (attachedStreams.TryGetValue(key, out var s))
                attachedStreams.Remove(key);
        }
    }
}