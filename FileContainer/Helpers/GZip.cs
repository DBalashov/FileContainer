using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace FileContainer
{
    static class GZipExtenders
    {
        [DebuggerStepThrough]
        [ContractAnnotation("buff: notnull => notnull; buff: null => canbenull")]
        public static byte[] GZipPack([CanBeNull] this byte[] buff)
        {
            if (buff == null)
                return null;
    
            using var stm = new MemoryStream();
            using (var gz = new GZipStream(stm, CompressionMode.Compress, true))
            {
                gz.Write(buff, 0, buff.Length);
                gz.Flush();
            }
    
            return stm.ToArray();
        }
    
        [DebuggerStepThrough]
        [ContractAnnotation("data: notnull => notnull; data: null => canbenull")]
        public static byte[] GZipUnpack([CanBeNull] this byte[] data)
        {
            if (data == null) return null;
    
            var buff = new byte[8192];
    
            using var stm = new MemoryStream();
            using (var st1 = new MemoryStream(data))
            {
                using (var gz = new GZipStream(st1, CompressionMode.Decompress, true))
                {
                    int read = 0;
                    while ((read = gz.Read(buff, 0, buff.Length)) > 0)
                        stm.Write(buff, 0, read);
                    gz.Close();
                }
    
                st1.Close();
            }
    
            var r = stm.ToArray();
            stm.Close();
    
            return r;
        }
    }
}