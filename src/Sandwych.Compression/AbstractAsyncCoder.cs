using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Compression;

public abstract class AbstractAsyncCoder : IAsyncCoder {
    public IReadOnlyDictionary<string, object> Options { get; private set; }

    public virtual void SetOptions(IReadOnlyDictionary<string, object> options) {
        this.Options = options ?? throw new ArgumentNullException(nameof(options));
    }

    public abstract ValueTask CodeAsync(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null);
}
