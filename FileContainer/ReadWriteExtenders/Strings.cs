using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FileContainer;

public static partial class ReadWriteExtenders
{
    static readonly Encoding defaultEncoding = Encoding.UTF8;

    #region Get

    /// <exception cref="ArgumentException"></exception>
    public static string? GetString(this PagedContainerAbstract c, string key)
    {
        var value = c.Get(key);
        return value == null ? null : defaultEncoding.GetString(value);
    }

    /// <summary> Get entries by keys. Mask chars * and ? supported in keys </summary>
    public static Dictionary<string, string> GetString(this PagedContainerAbstract c, params string[] keys) =>
        c.Get(keys).ToDictionary(p => p.Key,
                                 p => defaultEncoding.GetString(p.Value), StringComparer.InvariantCultureIgnoreCase);

    #endregion

    #region Put

    /// <summary> Create or replace entry with specified key </summary>
    /// <exception cref="ArgumentException"></exception>
    public static PutAppendResult Put(this PagedContainerAbstract c, string key, string data) =>
        string.IsNullOrEmpty(data)
            ? throw new ArgumentNullException(nameof(data), "Argument can't be null or empty")
            : c.Put(key, defaultEncoding.GetBytes(data));

    /// <summary>
    /// Create or replace of passed entries.
    /// Value in dictionary must not be null or empty.
    /// </summary>
    /// <exception cref="ArgumentException"></exception>
    public static Dictionary<string, PutAppendResult> Put(this PagedContainerAbstract c, Dictionary<string, string> keyValues)
    {
        checkDictionary(keyValues);
        return c.Put(keyValues.ToDictionary(p => p.Key, p => defaultEncoding.GetBytes(p.Value), StringComparer.InvariantCultureIgnoreCase));
    }

    #endregion

    #region Append

    public static PutAppendResult Append(this PagedContainerAbstract c, string key, string data) =>
        string.IsNullOrEmpty(data)
            ? throw new ArgumentNullException(nameof(data), "Argument can't be null or empty")
            : c.Append(key, defaultEncoding.GetBytes(data));


    public static Dictionary<string, PutAppendResult> Append(this PagedContainerAbstract c, Dictionary<string, string> keyValues)
    {
        checkDictionary(keyValues);
        return c.Append(keyValues.ToDictionary(p => p.Key, p => defaultEncoding.GetBytes(p.Value), StringComparer.InvariantCultureIgnoreCase));
    }

    #endregion

    static void checkDictionary(Dictionary<string, string> keyValues)
    {
        foreach (var item in keyValues)
        {
            if (string.IsNullOrEmpty(item.Key))
                throw new ArgumentNullException(nameof(keyValues), "Argument can't be null or empty");

            if (string.IsNullOrEmpty(item.Value))
                throw new ArgumentNullException(nameof(keyValues), $"Argument can't be null or empty: {item.Key}");
        }
    }
}