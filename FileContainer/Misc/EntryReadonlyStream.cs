using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace FileContainer
{
    [ExcludeFromCodeCoverage] // temporary, because in progress
    public class EntryReadonlyStream : Stream
    {
        [NotNull] readonly PagedContainerAbstract parent;
        [NotNull] readonly PagedContainerEntry    entry;
        [NotNull] readonly int[]                  pages;

        [NotNull] public PagedContainerEntry Entry => entry;

        internal EntryReadonlyStream([NotNull] PagedContainerAbstract parent, [NotNull] PagedContainerEntry entry)
        {
            this.parent = parent;
            this.entry  = entry;
            pages       = parent.stm.ReadPageSequence(parent.header, entry.FirstPage);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            #region checking parameters

            if (offset < 0)
                throw new ArgumentException("Parameter must be >= 0", nameof(offset));

            if (count <= 0)
                throw new ArgumentException("Parameter must be > 0", nameof(count));

            if (buffer == null)
                throw new ArgumentException("Buffer can't be null", nameof(buffer));

            if (offset + count > buffer.Length)
                throw new ArgumentException("Buffer too small (offset + count < buffer length)");

            #endregion

            var userDataLength = parent.header.PageUserDataSize;
            var offsetInPage   = (int) (Position - (Position / userDataLength) * userDataLength);

            if (Position + count > entry.Length)
                count = entry.Length - (int) Position;

            if (count <= 0) return 0;

            var readBytes = 0;
            foreach (var pageIndex in pages.Skip((int) Position / userDataLength))
            {
                parent.stm.Position = parent.PageSize * pageIndex + offsetInPage;

                var needToRead = count > (userDataLength - offsetInPage)
                    ? userDataLength - offsetInPage
                    : count;

                parent.stm.Read(buffer, offset, needToRead);
                offset += needToRead;
                count  -= needToRead;

                offsetInPage =  0;
                Position     += needToRead;
                readBytes    += needToRead;

                if (count == 0 || Position >= entry.Length) break;
            }

            return readBytes;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset < 0)
                throw new ArgumentException("Parameter can't be negative", nameof(offset));

            Position = origin switch
            {
                SeekOrigin.Begin => offset > entry.Length ? entry.Length : offset,              // если offset за границу размера - ставим в конец
                SeekOrigin.Current => Position + offset > entry.Length ? entry.Length : offset, // если position + offset за границу размера - ставим в конец 
                SeekOrigin.End => entry.Length - offset < 0 ? 0 : entry.Length - offset,        // если offset < 0 - ставим в 0
                _ => Position
            };

            return Position;
        }

        public override long Position { get; set; }

        public override void SetLength(long value)                         => throw new NotImplementedException();
        public override void Write(byte[]   buffer, int offset, int count) => throw new NotImplementedException();

        public override bool CanRead  => true;
        public override bool CanSeek  => true;
        public override bool CanWrite => false;

        public override void Flush()
        {
        }

        public override long Length => entry.Length;

        protected override void Dispose(bool disposing)
        {
            parent.DetachStream(entry.Name);
            base.Dispose(disposing);
        }

#if DEBUG
        public override string ToString() => $"{entry.Name}: Current position={Position}, Length={Length}";
#endif
    }
}