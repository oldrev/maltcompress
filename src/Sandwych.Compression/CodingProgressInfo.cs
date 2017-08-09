using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression
{
    public struct CodingProgressInfo
    {
        public long ProcessedInputSize { get; private set; }
        public long ProcessedOutputSize { get; private set; }

        public CodingProgressInfo(long processedInputSize, long processedOutputSize)
        {
            this.ProcessedInputSize = processedInputSize;
            this.ProcessedOutputSize = processedOutputSize;
        }
    }
}
