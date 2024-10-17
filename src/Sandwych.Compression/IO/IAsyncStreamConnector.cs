using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;

namespace Sandwych.Compression.IO;

public interface IAsyncStreamConnector : IAsyncDisposable
{
    Stream Producer { get; }
    Stream Consumer { get; }
    void Reset();
    ValueTask Produce(ReadOnlyMemory<byte> buffer, CancellationToken cancel);
    ValueTask<int> Consume(Memory<byte> buffer, CancellationToken cancel);

    ValueTask ProducerDisposed();
    ValueTask ConsumerDisposed();
}
