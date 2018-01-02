using Sandwych.Compression.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Sandwych.Compression
{
    internal class CodingStageThread
    {
        public Thread Thread { get; }
        public CodingThreadInfo Info { get; }
        public CodingStageThread(CodingThreadInfo info)
        {
            this.Info = info;
            this.Thread = new Thread(this.ThreadCallbackProc);
        }

        private void ThreadCallbackProc()
        {
            try
            {
                this.Info.Coder.Code(this.Info.InStream, this.Info.OutStream, this.Info.InSize, this.Info.OutSize, this.Info.Progress);
            }
            finally
            {
                if (this.Info.OutStream is ProducerStream upStream)
                {
                    upStream.Dispose();
                }
                if (this.Info.InStream is ConsumerStream downStream)
                {
                    downStream.Dispose();
                }
                this.Info.FinishedEvent.Set();
            }
        }
    }
}
