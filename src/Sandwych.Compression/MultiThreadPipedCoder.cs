using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Threading;
using Sandwych.Compression.IO;

namespace Sandwych.Compression
{

    public sealed class MultiThreadPipedCoder : ICoder
    {
        private readonly ICoder[] _coders;
        private readonly List<StreamConnector> _connectors;
        private readonly List<(Thread, CodingThreadInfo)> _threads;
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
                _threads = new List<(Thread, CodingThreadInfo)>(_coders.Length);

                var connectorCount = coders.Count() - 1;
                _connectors = new List<StreamConnector>(connectorCount);
                for (int i = 0; i < connectorCount; i++)
                {
                    _connectors.Add(new StreamConnector());
                }
            }
            else
            {
                _connectors = null;
            }

        }

        private void Reset()
        {
            _externalProgress = null;
            _processedInSize = 0;
            _processedOutSize = 0;
            if (_connectors != null)
            {
                foreach (var connector in _connectors)
                {
                    connector.Reinit();
                }
            }
        }

        public void Code(Stream inStream, Stream outStream, ICodingProgress progress = null)
        {
            this.Reset();
            _externalProgress = progress;

            if (_coders.Length > 1)
            {
                this.CreateAllThreads(inStream, outStream);

                //启动所有编码线程
                foreach (var t in _threads)
                {
                    t.Item1.Start(t.Item2);
                }

                var allFinishedEvents = _threads.Select(t => t.Item2.FinishedEvent.WaitHandle).ToArray();
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
            _threads.Add((new Thread(CodeProc), new CodingThreadInfo(_coders.First(), inStream, _connectors.First().UpStream, new ProcessedInSizeCodingProgress(this))));
            //last pair
            _threads.Add((new Thread(CodeProc), new CodingThreadInfo(_coders.Last(), _connectors.Last().DownStream, outStream, new ProcessedOutSizeCodingProgress(this))));

            if (_connectors.Count > 1)
            {
                var coders = _coders.Skip(1).Take(_coders.Count() - 2); //去掉头尾的
                var connectorIndex = 0;
                foreach (var coder in coders)
                {
                    var connector = _connectors[connectorIndex];
                    var nextConnector = _connectors[connectorIndex + 1];

                    _threads.Add((new Thread(CodeProc), new CodingThreadInfo(coder, connector.DownStream, nextConnector.UpStream, null)));
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
