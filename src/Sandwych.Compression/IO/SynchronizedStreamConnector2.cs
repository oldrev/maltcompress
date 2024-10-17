using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Sandwych.Compression.IO;

public class SynchronizedStreamConnector2 : IStreamConnector
{
	private ManualResetEventSlim _canConsumeEvent = new ManualResetEventSlim(false);

	private SemaphoreSlim _canProduceSempahore = new SemaphoreSlim(3);

	private volatile bool _downStreamClosed = false;
	private volatile bool _waitProducer = false;
	private volatile byte[] _buffer = null;
	private volatile int _bufferOffset = 0;
	private volatile int _bufferSize = 0;
	private bool _disposed = false;
	// private readonly WaitHandle[] _producerWaitHandles;
	private readonly ProducerStream _producerStream;
	private readonly ConsumerStream _consumerStream;

	public SynchronizedStreamConnector2()
	{
		_producerStream = new ProducerStream(this);
		_consumerStream = new ConsumerStream(this);
		// _producerWaitHandles = new WaitHandle[] { _canProduceEvent.WaitHandle, _consumerDisposedEvent.WaitHandle };
		this.Reset();
	}

	public void Reset()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException(nameof(SynchronizedStreamConnector));
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

			_canProduceSempahore.Wait();

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
					_canProduceSempahore.Release();
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
				this.CloseRead_CallOnce();

				_canConsumeEvent.Dispose();
				_canProduceSempahore.Dispose();
			}
		}
		_disposed = true;
	}

	public void Dispose()
	{
		this.Dispose(true);
		GC.SuppressFinalize(this);
	}

	~SynchronizedStreamConnector2()
	{
		this.Dispose(false);
	}

	void CloseRead_CallOnce()
	{
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

}
