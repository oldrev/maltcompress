﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO;

internal class AsyncConsumerStream : AbstractAsyncPipedStream
{
	public AsyncConsumerStream(IAsyncStreamConnector connector) : base(connector)
	{
	}

	public override bool CanRead => true;

	public override bool CanSeek => false;

	public override bool CanWrite => false;

	public override long Length => throw new NotSupportedException();

	public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

	public override void Flush() =>
		throw new NotSupportedException();

	public override int Read(byte[] buffer, int offset, int count) =>
		throw new NotSupportedException();

	public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
		throw new NotSupportedException();

	public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
	   this.Connector.Consume(buffer, cancellationToken);

	public override long Seek(long offset, SeekOrigin origin) =>
		throw new NotSupportedException();

	public override void SetLength(long value) =>
		throw new NotSupportedException();

	public override void Write(byte[] buffer, int offset, int count) =>
		throw new NotSupportedException();

	protected override void Dispose(bool disposing) =>
		this.Connector.ConsumerDisposed();
}
