using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression.IO
{
    internal class PipedDownStream : Stream
    {
        private readonly StreamConnector _pipe;

        public PipedDownStream(StreamConnector pipe)
        {
            _pipe = pipe;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _pipe.ReadFromDownStream(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _pipe.OnDownStreamClosed();
        }
    }
}
