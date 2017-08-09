using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression.IO
{
    internal class PipedUpStream : Stream
    {
        private readonly StreamConnector _pipe;

        public PipedUpStream(StreamConnector pipe)
        {
            _pipe = pipe;
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override void Flush()
        {
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
            _pipe.WriteToUpStream(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            _pipe.OnUpStreamClosed();
        }

    }
}
