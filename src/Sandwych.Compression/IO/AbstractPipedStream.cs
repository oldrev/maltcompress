using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sandwych.Compression.IO;

public abstract class AbstractPipedStream : Stream {
    public IStreamConnector Connector { get; private set; }

    public AbstractPipedStream(IStreamConnector connector) {
        this.Connector = connector ?? throw new ArgumentNullException(nameof(connector));
    }
}
