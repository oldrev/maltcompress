using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression.IO
{
    internal class PipedUpStream : Stream
    {
        private const int InternalBufferSize = 4096;
        private readonly DefaultStreamConnection _pipe;
        private long _position;
        private readonly Lazy<byte[]> _buffer = new Lazy<byte[]>(() => new byte[InternalBufferSize], true);
        private int _bufferSize = 0;
        private int _bufferOffset = 0;


        public PipedUpStream(DefaultStreamConnection pipe)
        {
            _pipe = pipe;
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => _position;
            set { throw new NotSupportedException(); }
        }

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
            _position += count;
        }

        protected override void Dispose(bool disposing)
        {
            _pipe.OnUpStreamClosed();
        }

    }
}
