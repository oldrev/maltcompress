using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNext.Threading;

namespace Sandwych.Compression.IO;

public class AsyncStreamConnector : IAsyncStreamConnector, IAsyncDisposable {
    private readonly AsyncManualResetEvent _canConsumeEvent = new AsyncManualResetEvent(false);

    private readonly SemaphoreSlim _canProduceSempahore = new SemaphoreSlim(0, 3);

    private volatile bool _downStreamClosed = false;
    private volatile bool _waitProducer = true;
    private ReadOnlyMemory<byte> _buffer;
    private bool _disposed = false;
    // private readonly WaitHandle[] _producerWaitHandles;
    private readonly AsyncProducerStream _producerStream;
    private readonly AsyncConsumerStream _consumerStream;

    public AsyncStreamConnector() {
        _producerStream = new AsyncProducerStream(this);
        _consumerStream = new AsyncConsumerStream(this);
        // _producerWaitHandles = new WaitHandle[] { _canProduceEvent.WaitHandle, _consumerDisposedEvent.WaitHandle };
        this.Reset();
    }

    public void Reset() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncStreamConnector));
        }

        _canConsumeEvent.Reset();
        if (_canProduceSempahore.CurrentCount > 0) {
            _canProduceSempahore.Release(_canProduceSempahore.CurrentCount);
        }

        _downStreamClosed = false;
        _waitProducer = true;
        _buffer = ReadOnlyMemory<byte>.Empty;
        this.ProcessedLength = 0;
    }

    public Stream Producer => _producerStream;

    public Stream Consumer => _consumerStream;

    public long ProcessedLength { get; private set; }

    public async ValueTask ProduceAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancel) {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncStreamConnector));
        }

        if (buffer.IsEmpty) {
            return;
        }

        if (!_downStreamClosed) {
            _buffer = buffer;
            var count = buffer.Length;

            //通知下游执行
            _canConsumeEvent.Set();

            await _canProduceSempahore.WaitAsync(cancel).ConfigureAwait(false);
            Debug.WriteLine("ProduceAsync go");

            count -= _buffer.Length;
            if (count > 0) {
                return;
            }

            _downStreamClosed = true;
        }

        throw new InvalidOperationException("Writing was cut");
    }

    public async ValueTask<int> ConsumeAsync(Memory<byte> buffer, CancellationToken cancel) {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncStreamConnector));
        }

        var count = buffer.Length;

        if (buffer.Length <= 0) {
            return 0;
        }

        if (_waitProducer) {
            await _canConsumeEvent.WaitAsync().ConfigureAwait(false);
            _waitProducer = false;
        }

        count = Math.Min(count, _buffer.Length); 

        if (count > 0) {
            Debug.WriteLine("ConsumeAsync go");
            _buffer.Slice(0, count).CopyTo(buffer);
            _buffer = _buffer.Slice(count, _buffer.Length - count);
            this.ProcessedLength += count;

            //下游已读取完毕，可以允许上游流写入了
            if (_buffer.Length == 0) {
                _waitProducer = true;
                _canProduceSempahore.Release();
            }
        }
        return count;
    }

    protected virtual void Dispose(bool disposing) {
        if (disposing) {
            if (!_disposed) {
                this.CloseRead_CallOnce();

                _canConsumeEvent.Dispose();
                _canProduceSempahore.Dispose();
            }
        }
        _disposed = true;
    }

    ~AsyncStreamConnector() {
        if (!_disposed) {
            throw new InvalidOperationException("Not disposed!");
        }
    }

    void CloseRead_CallOnce() {
        // call it only once: for example, in destructor

        /*
		_readingWasClosed = true;
		_canWrite_Event.Set();
		*/

        /*
		We must relase Semaphore only once !!!
		we must release at least 2 items of Semaphore:
		  one item to unlock partial Write(), if Read() have read some items
		  then additional item to stop writing (_bufSize will be 0)
		*/
        _canProduceSempahore.Release(2);
    }

    public ValueTask ProducerDisposedAsync() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncStreamConnector));
        }

        _buffer = ReadOnlyMemory<byte>.Empty;
        _canConsumeEvent.Set();
        return ValueTask.CompletedTask;
    }

    public ValueTask ConsumerDisposedAsync() {
        return ValueTask.CompletedTask;
    }

    public ValueTask DisposeAsync() {
        this.Dispose(true);
        GC.SuppressFinalize(this);
        return ValueTask.CompletedTask;
    }

}
