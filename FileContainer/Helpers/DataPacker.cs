using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using JetBrains.Annotations;

namespace FileContainer
{
    interface IDataHandler
    {
        [DebuggerStepThrough]
        [NotNull]
        byte[] Pack([NotNull] byte[] data);

        [DebuggerStepThrough]
        [NotNull]
        byte[] Unpack([NotNull] byte[] data);
    }

    class NoDataPacker : IDataHandler
    {
        public byte[] Pack(byte[]   data) => data;
        public byte[] Unpack(byte[] data) => data;
    }

    class GZipDataPacker : IDataHandler
    {
        readonly byte[] buff = new byte[8192];

        public byte[] Pack(byte[] data)
        {
            using var stm = new MemoryStream();
            using (var gz = new GZipStream(stm, CompressionMode.Compress, true))
            {
                gz.Write(data, 0, data.Length);
                gz.Flush();
            }

            return stm.ToArray();
        }

        public byte[] Unpack(byte[] data)
        {
            using var stm = new MemoryStream();
            using (var st1 = new MemoryStream(data))
            {
                using (var gz = new GZipStream(st1, CompressionMode.Decompress, true))
                {
                    int read;
                    while ((read = gz.Read(buff, 0, buff.Length)) > 0)
                        stm.Write(buff, 0, read);
                    gz.Close();
                }

                st1.Close();
            }

            return stm.ToArray();
        }
    }
}