using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Sandwych.Compression.Tests.Algorithms;

public sealed class AsyncXorCoder : AbstractAsyncCoder {
    private readonly byte _key;
    private readonly Lazy<byte[]> _buffer;

    public byte Key => _key;

    public AsyncXorCoder(byte key, int bufferSize = 65536) {
        _buffer = new Lazy<byte[]>(() => new byte[bufferSize], true);
        _key = key;
    }

    private void XorBlock(Memory<byte> buffer) {
        for (int i = 0; i < buffer.Length; i++) {
            buffer.Span[i] = (byte)(buffer.Span[i] ^ _key);
        }
    }

    public override async ValueTask CodeAsync(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null) {
        long processedSize = 0;
        for (; ; )
        {
            var inBuf = new Memory<byte>(_buffer.Value);
            var nRead = await inStream.ReadAsync(inBuf);
            if (nRead == 0) {
                break;
            }
            var outBuf = new Memory<byte>(_buffer.Value, 0, nRead);
            this.XorBlock(outBuf);
            await outStream.WriteAsync(outBuf);
            processedSize += nRead;
            if (progress != null) {
                progress.Report(new CodingProgressInfo(processedSize, processedSize));
            }
        }

    }
}
