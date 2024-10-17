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
    private AsyncManualResetEvent _canConsumeEvent = new AsyncManualResetEvent(false);

    private SemaphoreSlim _canProduceSempahore = new SemaphoreSlim(3);

    private volatile bool _downStreamClosed = false;
    private volatile bool _waitProducer = false;
    private ReadOnlyMemory<byte> _buffer;
    private volatile int _bufferOffset = 0;
    private volatile int _bufferSize = 0;
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
        _canProduceSempahore.Release();

        _downStreamClosed = false;
        _waitProducer = true;
        _buffer = null;
        _bufferOffset = 0;
        _bufferSize = 0;
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

        int count = buffer.Length;
        if (!_downStreamClosed) {
            _buffer = buffer;
            _bufferOffset = 0;
            _bufferSize = buffer.Length;

            //通知下游执行
            _canConsumeEvent.Set();

            await _canProduceSempahore.WaitAsync();

            count -= _bufferSize;
            if (count > 0) {
                return;
            }

            _downStreamClosed = true;
        }

        Debug.WriteLine("Writing was cut");
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
            await _canConsumeEvent.WaitAsync();
            _waitProducer = false;
        }

        if (count > _bufferSize) {
            count = _bufferSize;
        }

        if (count > 0) {
            _buffer.Slice(_bufferOffset, count).CopyTo(buffer);
            _bufferOffset += count;
            _bufferSize -= count;
            this.ProcessedLength += count;

            //下游已读取完毕，可以允许上游流写入了
            if (_bufferSize == 0) {
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

        _buffer = null;
        _bufferSize = 0;
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
