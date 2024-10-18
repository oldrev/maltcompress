using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Compression;

public sealed class AsyncCopyCoder : AbstractAsyncCoder {
    private readonly Lazy<byte[]> _buffer;

    public AsyncCopyCoder(int bufferSize = 65536) {
        _buffer = new Lazy<byte[]>(() => new byte[bufferSize], true);
    }

    public override async ValueTask CodeAsync(Stream inStream, Stream outStream, long inSize = -1, long outSize = -1, ICodingProgress progress = null) {
        long processedSize = 0;
        for (; ; )
        {
            var buf = new Memory<byte>(_buffer.Value);
            var nRead = await inStream.ReadAsync(buf).ConfigureAwait(false);
            if (nRead == 0) {
                break;
            }
            await outStream.WriteAsync(buf).ConfigureAwait(false);
            processedSize += nRead;
            if (progress != null) {
                progress.Report(new CodingProgressInfo(processedSize, processedSize));
            }
        }
    }
}
