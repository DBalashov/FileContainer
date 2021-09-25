using System;
using System.Collections.Generic;
using System.Linq;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <summary> Append data to end of existing entry. If entry doesn't exists - create new with passed data </summary>
        public virtual PutAppendResult Append(string key, byte[] data)
        {
            if (key == null)
                throw new ArgumentException("Argument can't be null", nameof(data));

            if (data == null)
                throw new ArgumentException("Argument can't be null", nameof(data));

            throwIfHasOpenedStream(new[] { key });
            if (key.ContainMask())
                throw new ArgumentException($"Invalid name: {key}");

            var r = append(key, data);

            if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
                WriteHeaders();

            return r;
        }

        /// <summary> Append data to end of passed entries. Create non-existing entries with passed data. Mask in keys not allowed. </summary>
        public virtual Dictionary<string, PutAppendResult> Append(Dictionary<string, byte[]> keyValues)
        {
            if (keyValues == null)
                throw new ArgumentException("Argument can't be null", nameof(keyValues));

            if (!keyValues.Any())
                return new Dictionary<string, PutAppendResult>();

            throwIfHasOpenedStream(keyValues.Keys);
            foreach (var item in keyValues)
            {
                if (string.IsNullOrEmpty(item.Key))
                    throw new ArgumentException("Argument can't be null or empty", nameof(keyValues));

                if (item.Value == null || item.Value.Length == 0)
                    throw new ArgumentException($"Argument can't be null or empty {item.Key}", nameof(keyValues));

                if (item.Key.ContainMask())
                    throw new ArgumentException($"Invalid name: {item.Key}");
            }

            var r = new Dictionary<string, PutAppendResult>();
            foreach (var item in keyValues)
                r.Add(item.Key, append(item.Key, item.Value));

            if (Flags.HasFlag(PersistentContainerFlags.WriteDirImmediately))
                WriteHeaders();

            return r;
        }

        PutAppendResult append(string key, byte[] data)
        {
            if (!entries.TryGet(key, out var existingEntry) || existingEntry == null)
            {
                put(key, data);
                return PutAppendResult.Created;
            }

            if (Header.CompressType != PersistentContainerCompressType.None)
                throw new NotSupportedException("Compressed container unsupported operation: Append");

            var userDataAtLastPage = existingEntry.Length % Header.PageUserDataSize; // rest of data on last page
            Stream.Position = existingEntry.LastPage * Header.PageSize + userDataAtLastPage;

            int lastPage;
            if (Header.PageUserDataSize - userDataAtLastPage >= data.Length) // is there enough free space on the last page for new data (no page allocation)?
            {
                Stream.Write(data, 0, data.Length);
                lastPage = existingEntry.LastPage;
            }
            else
            {
                var lengthToWriteOnExistingLastPage = Header.PageUserDataSize - userDataAtLastPage;
                Stream.Write(data, 0, lengthToWriteOnExistingLastPage); // head of data write to free space on last page 

                var remainDataLength = data.Length - lengthToWriteOnExistingLastPage;
                if (remainDataLength > 0)
                {
                    var additionalPages = pageAllocator.AllocatePages(Header.GetRequiredPages(remainDataLength));

                    Stream.Write(BitConverter.GetBytes(additionalPages.First()), 0, 4); // link on first new allocated page
                    Stream.WriteIntoPages(Header, data, lengthToWriteOnExistingLastPage, additionalPages);
                    lastPage = additionalPages.Last();
                }
                else lastPage = existingEntry.LastPage;
            }

            entries.Update(existingEntry,
                           existingEntry.FirstPage, lastPage,
                           existingEntry.Length + data.Length,
                           existingEntry.Length + data.Length,
                           0);
            return PutAppendResult.Updated;
        }
    }
}