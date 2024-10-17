using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Compression;

public interface IAsyncCoder {
    IReadOnlyDictionary<string, object> Options { get; }

    void SetOptions(IReadOnlyDictionary<string, object> options);

    ValueTask CodeAsync(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null);
}
