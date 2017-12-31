using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sandwych.Compression.IO
{
    public abstract class AbstractPipedStream : Stream
    {
        public IStreamConnector Connector { get; private set; }

        public AbstractPipedStream(IStreamConnector connector)
        {
            this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
        }
    }
}
