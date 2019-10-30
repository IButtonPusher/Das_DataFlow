using System;
using DataFlow.Interfaces;

namespace Das.DataFlow
{
    public abstract class SeriesProcessor : ISeriesProcessor
    {
        public Double PercentComplete => _progressReporter.PercentComplete;

        public abstract Boolean IsFinished { get; }

        public abstract Boolean HasDataSource { get; }

        public virtual Boolean HasAvailableTask => _bufferManager.HasAvailableTask;

        public Boolean IsActivelyWorking { get; protected set; }

        public Int32 TotalProcessed
        {
            get => _bufferManager.TotalProcessed;
            protected set => _bufferManager.TotalProcessed = value;
        }
        public Int32 TotalPublished
        {
            get => _bufferManager.TotalPublished;
            private set => _bufferManager.TotalPublished = value;
        }

        public Int32? AvailableBuffer => _bufferManager.AvailableBuffer;

        public Int32? MaximumBuffer => _bufferManager.MaximumBuffer;

        public Int32 CurrentBufferSize => _bufferManager.BufferSize;
        Boolean ISeriesProcessor.IsStarted { get; set; }

        protected readonly IProgressive _progressReporter;
        private readonly IBufferManager _bufferManager;
        private readonly IWorkManager _workManager;

        protected SeriesProcessor(IProgressive progressReporter, IBufferManager bufferManager,
            IWorkManager workManager)
        {
            _progressReporter = progressReporter;
            _bufferManager = bufferManager;
            _workManager = workManager;
        }

        public Boolean TryProcess(Int32? maxToPublish)
        {
            IsActivelyWorking = true;
            try
            {
                if (!maxToPublish.HasValue)
                    return TryProcessAll();

                return TryProcessSome(maxToPublish.Value);
            }
            finally { IsActivelyWorking = false; }
        }


        protected Boolean FinishWork(Int32 published)
        {
            if (published == 0)
                return false;
            TotalPublished += published;
            return true;
        }

        public void Dispose()
        {
            _workManager?.Dispose();
        }

        protected abstract Boolean TryProcessAll();
        protected abstract Boolean TryProcessSome(Int32 maxToPublish);
    }

	
}