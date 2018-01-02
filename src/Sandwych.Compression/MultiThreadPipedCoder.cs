using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Sandwych.Compression.IO;

namespace Sandwych.Compression
{
   

    public sealed class MultiThreadPipedCoder : AbstractCoder
    {
        private readonly ICoder[] _coders;
        private readonly List<IStreamConnector> _connections;
        private readonly List<CodingStageThread> _threads;
        private long _processedInSize;
        private long _processedOutSize;
        private ICodingProgress _externalProgress;

        struct ProcessedInSizeCodingProgress : ICodingProgress
        {
            private readonly MultiThreadPipedCoder _pipedCoder;

            public ProcessedInSizeCodingProgress(MultiThreadPipedCoder pipedCoder)
            {
                _pipedCoder = pipedCoder;
            }

            public void Report(CodingProgressInfo value)
            {
                _pipedCoder._processedInSize = value.ProcessedInputSize;
            }
        }

        struct ProcessedOutSizeCodingProgress : ICodingProgress
        {
            private readonly MultiThreadPipedCoder _pipedCoder;

            public ProcessedOutSizeCodingProgress(MultiThreadPipedCoder pipedCoder)
            {
                _pipedCoder = pipedCoder;
            }

            public void Report(CodingProgressInfo value)
            {
                _pipedCoder._processedOutSize = value.ProcessedOutputSize;
                _pipedCoder.ReportProgress();
            }
        }

        public MultiThreadPipedCoder(params ICoder[] coders)
        {
            if (coders == null || coders.Count() == 0)
            {
                _coders = new ICoder[] { new PassThroughCoder() };
            }
            else
            {
                _coders = coders;
            }

            if (_coders.Length > 1)
            {
                _threads = new List<CodingStageThread>(_coders.Length);

                var connectionCount = coders.Count() - 1;
                _connections = new List<IStreamConnector>(connectionCount);
                for (int i = 0; i < connectionCount; i++)
                {
                    _connections.Add(new MultiThreadedStreamConnector());
                }
            }
            else
            {
                _connections = null;
            }

        }

        private void Reset()
        {
            _externalProgress = null;
            _processedInSize = 0;
            _processedOutSize = 0;
            if (_connections != null)
            {
                foreach (var connector in _connections)
                {
                    connector.Reset();
                }
            }
        }

        public override void Code(Stream inStream, Stream outStream, long inSize, long outSize, ICodingProgress progress = null)
        {
            this.Reset();
            _externalProgress = progress;

            if (_coders.Length > 1)
            {
                this.CreateAllCodingThreads(inStream, outStream, inSize, outSize);

                foreach (var t in _threads)
                {
                    t.Thread.Start();
                }

                var allFinishedEvents = _threads.Select(t => t.Info.FinishedEvent.WaitHandle).ToArray();
                WaitHandle.WaitAll(allFinishedEvents);
            }
            else
            {
                _coders.First().Code(inStream, outStream, inSize, outSize, progress);
            }
        }

        private void CreateAllCodingThreads(Stream inStream, Stream outStream, long inSize, long outSize)
        {
            _threads.Clear();

            //first pair
            _threads.Add(new CodingStageThread(new CodingThreadInfo(_coders.First(), inStream, _connections.First().Producer, inSize, outSize, new ProcessedInSizeCodingProgress(this))));
            //last pair
            _threads.Add(new CodingStageThread(new CodingThreadInfo(_coders.Last(), _connections.Last().Consumer, outStream, inSize, outSize, new ProcessedOutSizeCodingProgress(this))));

            if (_connections.Count > 1)
            {
                var coders = _coders.Skip(1).Take(_coders.Count() - 2); //removed head and tail
                var connectorIndex = 0;
                foreach (var coder in coders)
                {
                    var connector = _connections[connectorIndex];
                    var nextConnector = _connections[connectorIndex + 1];

                    var thread = new CodingStageThread(new CodingThreadInfo(coder, connector.Consumer, nextConnector.Producer, inSize, outSize, null));
                    _threads.Add(thread);
                    connectorIndex++;
                }
            }
        }

        private static void CodingProc(object args)
        {
            var info = args as CodingThreadInfo;
            info.Coder.Code(info.InStream, info.OutStream, info.InSize, info.OutSize, info.Progress);
            if (info.OutStream is ProducerStream upStream)
            {
                upStream.Dispose();
            }
            if (info.InStream is ConsumerStream downStream)
            {
                downStream.Dispose();
            }
            info.FinishedEvent.Set();
        }

        private void ReportProgress()
        {
            if (_externalProgress != null)
            {
                _externalProgress.Report(new CodingProgressInfo(_processedInSize, _processedOutSize));
            }
        }
    }
}
