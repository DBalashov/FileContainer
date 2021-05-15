using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace FileContainer
{
    /// <summary>
    /// <code>
    /// Key/value storage as single file
    ///
    /// * data stored with pages
    /// * page size can be choose only while creating of new container
    /// * keys always stored as case-insensitive UTF-8 strings
    /// * keys can be like dir/subdir/file1.txt
    /// + supported of chars * and ? for querying:
    ///     dir/subdir/*
    ///     dir/subdir/file?.txt
    ///     dir/*/*.txt
    ///     dir/*
    /// + supported batch put/delete/append (faster 2-3 times in compare one-by-one)
    /// * support put/delete/append for byte[] and strings
    /// * multiple updated values will write to same pages
    ///     if new data more than existing - will allocated new pages
    ///     if new data less then existing - pages freed
    /// * pages marked as 'free' while deleting keys. container length don't changed (!)
    /// * max size of single entry: 2 GB (int.MaxValue)
    /// * max size of key: 65535 (ushort) bytes in UTF-8
    /// * file container created/opened in exclusive mode
    /// * for concurrent reading can be used PersistentReadonlyContainer  
    /// </code>
    /// <code>
    /// todo: +optional per-page compression
    /// todo: +trim operation for shrink container
    /// todo: +metadata for each entry and container
    /// todo: optional case-sensitive keys
    /// </code>
    /// </summary>
    public class PersistentContainer : PagedContainerAbstract
    {
        public PersistentContainer([NotNull] string fileName, int pageSize = 4096, PersistentContainerFlags flags = 0) :
            base(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None, pageSize * 2), pageSize)
        {
        }
    }

    [Flags]
    public enum PersistentContainerFlags
    {
        WriteDirImmediately = 1,
        Compressed          = 2
    }
}