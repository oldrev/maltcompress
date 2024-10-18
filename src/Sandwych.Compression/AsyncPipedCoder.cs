using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Sandwych.Compression.IO;

namespace Sandwych.Compression;


public class AsyncPipedCoder<TConnector> : AbstractAsyncCoder, IAsyncDisposable
    where TConnector : IAsyncStreamConnector, new() {
    private bool _disposed = false;
    private readonly IAsyncCoder[] _coders;
    private readonly List<TConnector> _connections;
    private readonly List<ValueTask> _tasks;
    private long _processedInSize;
    private long _processedOutSize;
    private ICodingProgress _externalProgress;

    struct ProcessedInSizeCodingProgress : ICodingProgress {
        private readonly AsyncPipedCoder<TConnector> _pipedCoder;

        public ProcessedInSizeCodingProgress(AsyncPipedCoder<TConnector> pipedCoder) {
            _pipedCoder = pipedCoder;
        }

        public void Report(CodingProgressInfo value) {
            _pipedCoder._processedInSize = value.ProcessedInputSize;
        }
    }

    struct ProcessedOutSizeCodingProgress : ICodingProgress {
        private readonly AsyncPipedCoder<TConnector> _pipedCoder;

        public ProcessedOutSizeCodingProgress(AsyncPipedCoder<TConnector> pipedCoder) {
            _pipedCoder = pipedCoder;
        }

        public void Report(CodingProgressInfo value) {
            _pipedCoder._processedOutSize = value.ProcessedOutputSize;
            _pipedCoder.ReportProgress();
        }
    }

    public AsyncPipedCoder(params IAsyncCoder[] coders) {
        if (coders == null || coders.Count() == 0) {
            _coders = [new AsyncCopyCoder()];
        }
        else {
            _coders = coders;
        }

        if (_coders.Length > 1) {
            _tasks = new List<ValueTask>(_coders.Length);

            var connectionCount = coders.Count() - 1;
            _connections = new List<TConnector>(connectionCount);
            for (int i = 0; i < connectionCount; i++) {
                _connections.Add(new TConnector());
            }
        }
        else {
            _connections = null;
        }

    }

    private void Reset() {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncPipedCoder<TConnector>));
        }
        _externalProgress = null;
        _processedInSize = 0;
        _processedOutSize = 0;
        if (_connections != null) {
            foreach (var connector in _connections) {
                connector.Reset();
            }
        }
    }

    public override async ValueTask CodeAsync(Stream inStream, Stream outStream,
        long inSize = -1, long outSize = -1, ICodingProgress progress = null) {
        if (_disposed) {
            throw new ObjectDisposedException(nameof(AsyncPipedCoder<TConnector>));
        }

        this.Reset();
        _externalProgress = progress;

        if (_coders.Length > 1) {
            this.CreateAllCodingTasks(inStream, outStream, inSize, outSize);

            var tasks = _tasks.Select(x => x.AsTask()).ToArray();
            await Task.WhenAll(tasks).ConfigureAwait(false);
            //var allFinishedEvents = _tasks.Select(t => t.Info.FinishedEvent.WaitHandle).ToArray();
            //WaitHandle.WaitAll(allFinishedEvents);
        }
        else {
            await _coders.First().CodeAsync(inStream, outStream, inSize, outSize, progress).ConfigureAwait(false);
        }
    }

    private void CreateAllCodingTasks(Stream inStream, Stream outStream, long inSize, long outSize) {
        _tasks.Clear();

        //first stage
        _tasks.Add(_coders.First().CodeAsync(inStream, _connections.First().Producer, inSize, outSize, new ProcessedInSizeCodingProgress(this)));
        //last stage
        _tasks.Add(_coders.Last().CodeAsync(_connections.Last().Consumer, outStream, inSize, outSize, new ProcessedInSizeCodingProgress(this)));

        if (_connections.Count > 1) {
            var coders = _coders.Skip(1).Take(_coders.Length - 2); //removed head and tail
            var connectorIndex = 0;
            foreach (var coder in coders) {
                var connector = _connections[connectorIndex];
                var nextConnector = _connections[connectorIndex + 1];

                var task = coder.CodeAsync(connector.Consumer, nextConnector.Producer, inSize, outSize, null);
                _tasks.Add(task);
                connectorIndex++;
            }
        }
    }

    private void ReportProgress() {
        if (_externalProgress != null) {
            _externalProgress.Report(new CodingProgressInfo(_processedInSize, _processedOutSize));
        }
    }

    protected async virtual ValueTask DisposeAsync(bool disposing) {
        if (disposing) {
            if (!_disposed) {
                if (_connections != null && _connections.Count > 0) {
                    foreach (var connector in _connections) {
                        await connector.DisposeAsync().ConfigureAwait(false);
                    }
                }
            }
        }
        _disposed = true;
    }

    public ValueTask DisposeAsync() =>
        this.DisposeAsync(true);

}
