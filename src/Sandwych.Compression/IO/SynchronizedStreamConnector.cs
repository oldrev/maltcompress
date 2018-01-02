using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO
{
    public class SynchronizedStreamConnector : IStreamConnector
    {
        private readonly ManualResetEventSlim _canProduceEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _canConsumeEvent = new ManualResetEventSlim(false);
        private readonly ManualResetEventSlim _consumerDisposedEvent = new ManualResetEventSlim(false);
        private volatile bool _downStreamClosed = false;
        private volatile bool _waitProducer = false;
        private volatile byte[] _buffer = null;
        private volatile int _bufferOffset = 0;
        private volatile int _bufferSize = 0;
        private bool _disposed = false;
        private readonly WaitHandle[] _producerWaitHandles;
        private readonly ProducerStream _producerStream;
        private readonly ConsumerStream _consumerStream;

        public SynchronizedStreamConnector()
        {
            _producerStream = new ProducerStream(this);
            _consumerStream = new ConsumerStream(this);
            _producerWaitHandles = new WaitHandle[] { _canProduceEvent.WaitHandle, _consumerDisposedEvent.WaitHandle };
            this.Reset();
        }

        public void Reset()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
            }

            _canProduceEvent.Reset();
            _canConsumeEvent.Reset();
            _consumerDisposedEvent.Reset();
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

        public void Produce(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
            }
            this.ProduceInternal(buffer, offset, count);
        }

        private void ProduceInternal(byte[] buffer, int offset, int count)
        {
            if (count == 0)
            {
                return;
            }

            if (!_downStreamClosed)
            {
                _buffer = buffer;
                _bufferOffset = offset;
                _bufferSize = count;

                //通知下游执行
                _canConsumeEvent.Set();

                var waitResult = WaitHandle.WaitAny(_producerWaitHandles);
                if (waitResult == 0)
                {
                    _canProduceEvent.Reset();
                }

                count -= _bufferSize;
                if (count > 0)
                {
                    return;
                }

                _downStreamClosed = true;
            }

            throw new IOException("Writing was cut");
        }

        public int Consume(byte[] buffer, int offset, int count)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
            }

            return this.ConsumeInternal(buffer, offset, count);
        }

        private int ConsumeInternal(byte[] buffer, int offset, int count)
        {
            var consumerDesiredSize = count;
            var nRead = 0;

            if (count <= 0)
            {
                return 0;
            }

            while (nRead < consumerDesiredSize)
            {
                if (_waitProducer)
                {
                    _canConsumeEvent.Wait();
                    _waitProducer = false;
                }

                count = Math.Min(consumerDesiredSize - nRead, _bufferSize);
                if (count > 0)
                {
                    Buffer.BlockCopy(_buffer, _bufferOffset, buffer, offset, count);
                    offset += count;
                    _bufferOffset += count;
                    _bufferSize -= count;
                    nRead += count;
                    this.ProcessedLength += nRead;
                    //下游已读取完毕，可以允许上游流写入了
                    if (_bufferSize == 0)
                    {
                        _waitProducer = true;
                        _canConsumeEvent.Reset();
                        _canProduceEvent.Set();
                    }
                }
                else if (count == 0)
                {
                    break;
                }
                else
                {
                    throw new InvalidOperationException();
                }
            }
            return nRead;
        }


        /// <summary>
        /// 下游已关闭
        /// </summary>
        public void ConsumerDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
            }

            _consumerDisposedEvent.Set();
        }

        /// <summary>
        /// 上游已关闭
        /// </summary>
        public void ProducerDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
            }

            _buffer = null;
            _bufferSize = 0;
            _canConsumeEvent.Set();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!_disposed)
                {
                    _canProduceEvent.Dispose();
                    _canConsumeEvent.Dispose();
                    _consumerDisposedEvent.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~SynchronizedStreamConnector()
        {
            this.Dispose(false);
        }
    }
}
