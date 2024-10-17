using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO;

public interface IAsyncStreamConnector : IAsyncDisposable {
    Stream Producer { get; }
    Stream Consumer { get; }
    void Reset();
    ValueTask ProduceAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancel);
    ValueTask<int> ConsumeAsync(Memory<byte> buffer, CancellationToken cancel);

    ValueTask ProducerDisposedAsync();
    ValueTask ConsumerDisposedAsync();
}
