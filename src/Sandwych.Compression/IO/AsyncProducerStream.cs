using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO;

internal class AsyncProducerStream : AbstractAsyncPipedStream {
    private const int InternalBufferSize = 4096;
    private long _position;
    //private readonly Lazy<byte[]> _buffer = new Lazy<byte[]>(() => new byte[InternalBufferSize], true);


    public AsyncProducerStream(IAsyncStreamConnector connector) : base(connector) {
    }

    public override bool CanRead => false;

    public override bool CanSeek => true;

    public override bool CanWrite => true;

    public override long Length => throw new NotSupportedException();

    public override long Position {
        get => _position;
        set => throw new NotSupportedException();
    }

    public override int Read(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();

    public override int Read(Span<byte> buffer) =>
        throw new NotSupportedException();

    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        throw new NotSupportedException();

    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        throw new NotSupportedException();

    public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
        throw new NotSupportedException();

    public override void Flush() {
    }

    public override long Seek(long offset, SeekOrigin origin) =>
        throw new NotSupportedException();

    public override void SetLength(long value) =>
        throw new NotSupportedException();

    public override async ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) {
        await this.Connector.ProduceAsync(buffer, cancellationToken);
        _position += buffer.Length;
    }

    public override void Write(byte[] buffer, int offset, int count) =>
        throw new NotSupportedException();


    public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
         await this.Connector.ProduceAsync(new ReadOnlyMemory<byte>(buffer, offset, count), cancellationToken);

    public override void Write(ReadOnlySpan<byte> buffer) =>
        throw new NotSupportedException();

    public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state) =>
        throw new NotSupportedException();

    protected override void Dispose(bool disposing) =>
        this.Connector.ProducerDisposedAsync();

}
