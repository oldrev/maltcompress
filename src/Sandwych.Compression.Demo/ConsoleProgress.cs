using System;
using System.Collections.Generic;
using System.Text;

namespace Sandwych.Compression.Demo
{
    public class ConsoleProgress : ICodingProgress
    {
        public long CurrentStreamSize { get; private set; }

        public ConsoleProgress(long currentStreamSize)
        {
            this.CurrentStreamSize = currentStreamSize;
        }


        public void Report(CodingProgressInfo value)
        {
            Console.Write("\r");
            Console.Write("Compressing: {0}%", Math.Round((double)value.ProcessedInputSize * 100.0 / (double)this.CurrentStreamSize));
        }
    }
}
