using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO
{
    internal class ProducerStream : AbstractPipedStream
    {
        private const int InternalBufferSize = 4096;
        private long _position;
        //private readonly Lazy<byte[]> _buffer = new Lazy<byte[]>(() => new byte[InternalBufferSize], true);


        public ProducerStream(IStreamConnector connector) : base(connector)
        {
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
            this.Connector.Produce(buffer, offset, count);
            _position += count;
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            throw new NotSupportedException();
        }

        protected override void Dispose(bool disposing)
        {
            this.Connector.ProducerDisposed();
        }

    }
}
