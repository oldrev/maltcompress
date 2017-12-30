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
        private readonly struct ThreadPair
        {
            public Thread Thread { get; }
            public CodingThreadInfo Info { get; }
            public ThreadPair(Thread thread, CodingThreadInfo info)
            {
                this.Thread = thread;
                this.Info = info;
            }
        }

        private readonly ICoder[] _coders;
        private readonly List<IStreamConnection> _connections;
        private readonly List<ThreadPair> _threads;
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
                _threads = new List<ThreadPair>(_coders.Length);

                var connectionCount = coders.Count() - 1;
                _connections = new List<IStreamConnection>(connectionCount);
                for (int i = 0; i < connectionCount; i++)
                {
                    _connections.Add(new DefaultStreamConnection());
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

        public override void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            this.Reset();
            _externalProgress = progress;

            if (_coders.Length > 1)
            {
                this.CreateAllThreads(inStream, outStream);

                //启动所有编码线程
                //这里为了安全考虑，需要从反向的开始，确保所有的线程都是先停止，再由最上游的触发
                var reversedThreads = _threads.Reverse<ThreadPair>();
                foreach (var t in reversedThreads)
                {
                    t.Thread.Start(t.Info);
                }

                var allFinishedEvents = _threads.Select(t => t.Info.FinishedEvent.WaitHandle).ToArray();
                WaitHandle.WaitAll(allFinishedEvents);
            }
            else
            {
                _coders.First().Code(inStream, outStream, progress);
            }
        }

        private void CreateAllThreads(Stream inStream, Stream outStream)
        {
            _threads.Clear();

            //first pair
            _threads.Add(new ThreadPair(new Thread(CodeProc), new CodingThreadInfo(_coders.First(), inStream, _connections.First().UpStream, new ProcessedInSizeCodingProgress(this))));
            //last pair
            _threads.Add(new ThreadPair(new Thread(CodeProc), new CodingThreadInfo(_coders.Last(), _connections.Last().DownStream, outStream, new ProcessedOutSizeCodingProgress(this))));

            if (_connections.Count > 1)
            {
                var coders = _coders.Skip(1).Take(_coders.Count() - 2); //去掉头尾的
                var connectorIndex = 0;
                foreach (var coder in coders)
                {
                    var connector = _connections[connectorIndex];
                    var nextConnector = _connections[connectorIndex + 1];

                    var pair = new ThreadPair(new Thread(CodeProc), new CodingThreadInfo(coder, connector.DownStream, nextConnector.UpStream, null));
                    _threads.Add(pair);
                    connectorIndex++;
                }
            }
        }

        private static void CodeProc(object args)
        {
            var info = args as CodingThreadInfo;
            info.Coder.Code(info.InStream, info.OutStream, info.Progress);
            if (info.OutStream is PipedUpStream upStream)
            {
                upStream.Dispose();
            }
            if (info.InStream is PipedDownStream downStream)
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
