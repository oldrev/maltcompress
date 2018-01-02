using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sandwych.Compression.IO
{
    public interface IStreamConnector : IDisposable
    {
        Stream Producer { get; }
        Stream Consumer { get; }
        void Reset();
        void Produce(byte[] buffer, int offset, int count);
        int Consume(byte[] buffer, int offset, int count);

        void ProducerDisposed();
        void ConsumerDisposed();
    }
}
