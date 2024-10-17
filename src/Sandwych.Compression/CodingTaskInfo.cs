using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace Sandwych.Compression;

internal class CodingTaskInfo {
    public CodingTaskInfo(ICoder coder, Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null) {
        this.Coder = coder;
        this.InStream = inStream;
        this.OutStream = outStream;
        this.Progress = progress;
        this.InSize = inSize;
        this.OutSize = outSize;
    }

    public ICoder Coder { get; private set; }
    public Stream InStream { get; private set; }
    public Stream OutStream { get; private set; }
    public long InSize { get; private set; }
    public long OutSize { get; private set; }
    public ICodingProgress Progress { get; private set; }

}
