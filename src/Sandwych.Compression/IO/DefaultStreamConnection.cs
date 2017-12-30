using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO
{
    internal class DefaultStreamConnection : IStreamConnection
    {

        private readonly AutoResetEvent _canWriteEvent = new AutoResetEvent(false);
        private readonly ManualResetEvent _canReadEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent _downStreamClosedEvent = new ManualResetEvent(false);
        private volatile bool _downStreamClosed = false;
        private volatile bool _waitWrite = false;
        private volatile byte[] _buffer = null;
        private volatile int _bufferOffset = 0;
        private volatile int _bufferSize = 0;
        private readonly WaitHandle[] _writeEvents;
        private readonly PipedUpStream _upStream;
        private readonly PipedDownStream _downStream;

        public DefaultStreamConnection()
        {
            _upStream = new PipedUpStream(this);
            _downStream = new PipedDownStream(this);
            _writeEvents = new WaitHandle[] { _canWriteEvent, _downStreamClosedEvent };
            this.Reset();
        }

        public void Reset()
        {
            _canWriteEvent.Reset();
            _canReadEvent.Reset();
            _downStreamClosedEvent.Reset();
            _downStreamClosed = false;
            _waitWrite = true;
            _buffer = null;
            _bufferOffset = 0;
            _bufferSize = 0;
            this.ProcessedLength = 0;
        }

        public Stream UpStream => _upStream;

        public Stream DownStream => _downStream;

        public long ProcessedLength { get; private set; }

        public int ReadFromDownStream(Span<byte> span)
        {
            return this.ReadFromDownStream(span.ToArray(), 0, span.Length);
        }

        public int ReadFromDownStream(byte[] buffer, int offset, int count)
        {
            var downStreamDesiredSize = count;
            var nRead = 0;

            if (count > 0)
            {
                while (nRead < downStreamDesiredSize)
                {
                    if (_waitWrite)
                    {
                        _canReadEvent.WaitOne();
                        _waitWrite = false;
                    }

                    count = Math.Min(downStreamDesiredSize - nRead, _bufferSize);
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
                            _waitWrite = true;
                            _canReadEvent.Reset();
                            _canWriteEvent.Set();
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
            }
            return nRead;
        }

        public void WriteToUpStream(byte[] buffer, int offset, int count)
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
                _canReadEvent.Set();

                var waitResult = WaitHandle.WaitAny(_writeEvents);

                count -= _bufferSize;
                if (count > 0)
                {
                    return;
                }

                _downStreamClosed = true;
            }

            throw new IOException("Writing was cut");
        }

        /// <summary>
        /// 下游已关闭
        /// </summary>
        public void OnDownStreamClosed()
        {
            _downStreamClosedEvent.Set();
        }

        /// <summary>
        /// 上游已关闭
        /// </summary>
        public void OnUpStreamClosed()
        {
            _buffer = null;
            _bufferSize = 0;
            _canReadEvent.Set();
        }


    }
}
