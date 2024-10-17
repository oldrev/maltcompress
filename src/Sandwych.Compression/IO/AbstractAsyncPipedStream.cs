using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace Sandwych.Compression.IO;

public abstract class AbstractAsyncPipedStream : Stream
{
    public IAsyncStreamConnector Connector { get; private set; }

    public AbstractAsyncPipedStream(IAsyncStreamConnector connector)
    {
        this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
}
