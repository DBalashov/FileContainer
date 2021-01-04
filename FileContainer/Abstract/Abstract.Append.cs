using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    public abstract partial class PagedContainerAbstract
    {
        /// <summary> Append data to end of existing entry. If entry doesn't exists - create new with passed data </summary>
        public virtual PutAppendResult Append([NotNull] string key, [NotNull] byte[] data)
        {
            if (key == null)
                throw new ArgumentException("Argument can't be null", nameof(data));

            if (data == null)
                throw new ArgumentException("Argument can't be null", nameof(data));

            throwIfHasOpenedStream(new[] {key});
            if (key.ContainMask())
                throw new ArgumentException($"Invalid name: {key}");

            var r = append(key, data);
            return r;
        }

        /// <summary> Append data to end of passed entries. Create non-existing entries with passed data. Mask in keys not allowed. </summary>
        [NotNull]
        public virtual Dictionary<string, PutAppendResult> Append([NotNull] Dictionary<string, byte[]> keyValues)
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

            return r;
        }

        PutAppendResult append([NotNull] string key, [NotNull] byte[] data)
        {
            if (!entries.TryGet(key, out var existingEntry))
            {
                put(key, data);
                return PutAppendResult.Created;
            }

            var userDataAtLastPage = existingEntry.Length % header.PageUserDataSize; // остаток данных на последней странице
            stm.Position = (existingEntry.LastPage * header.PageSize) + userDataAtLastPage;

            int lastPage;
            if (header.PageUserDataSize - userDataAtLastPage >= data.Length) // дописываемые данные укладываются на последнюю страницу без выделений новых страниц?
            {
                stm.Write(data, 0, data.Length);
                lastPage = existingEntry.LastPage;
            }
            else
            {
                var lengthToWriteOnExistingLastPage = header.PageUserDataSize - userDataAtLastPage;
                stm.Write(data, 0, lengthToWriteOnExistingLastPage); // голову данных пишем на последнюю страницу в свободное место 

                var remainDataLength = data.Length - lengthToWriteOnExistingLastPage;
                if (remainDataLength > 0)
                {
                    var additionalPages = pageAllocator.AllocatePages(header.GetRequiredPages(remainDataLength));

                    stm.Write(BitConverter.GetBytes(additionalPages.First()), 0, 4); // ссылка на первую новую выделенную страницу
                    stm.WriteIntoPages(header, data, lengthToWriteOnExistingLastPage, additionalPages);
                    lastPage = additionalPages.Last();
                }
                else lastPage = existingEntry.LastPage;
            }

            existingEntry.Length   += data.Length;
            existingEntry.Modified =  DateTime.UtcNow;
            existingEntry.LastPage =  lastPage;
            return PutAppendResult.Updated;
        }
    }
}