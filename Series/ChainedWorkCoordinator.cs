using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Das.DataCore;

namespace Das.DataFlow
{
    internal class ChainedWorkCoordinator<TInput> : CoreCoordinator, IWorkerCoordinator, 
		IWeightedDataSubscriber<TInput>, ISeriesMaster, IProgressNotifier
	{
		public override Boolean IsStarted => _publishers.IsStarted;

		public override Boolean IsFinished => _publishers?.IsFinished == true &&
			!_pendingRecords.Any() && 
			!_seriesProcessors.Any(s => s.IsActivelyWorking) &&
		    _recordsHavingStarted == _recordsProcessed;

		public override Boolean IsCancelled => _publishers.IsCancelled;

		public override event EventHandler<Double> ProgressChanged;
		public override event EventHandler Starting;
		public override event EventHandler Finishing;
		public override event EventHandler Finished;

		private Double _percentComplete;
		private readonly List<IProgressive> _progressives;
		private readonly EventArgs _nullArgs;

		private Int64 _totalWorkLoad;
		private Int64 _workLoadInProgress;
		private Int64 _completedWorkLoad;

		public override Double PercentComplete => _percentComplete + WorkInProgressPercent;

		private Double WorkInProgressPercent
		{
			get
			{
				if (_progressives.Count == 0 || _recordsHavingStarted == 0)
					return 0;
				Double workingOn = _recordsHavingStarted - _recordsProcessed;
				if (workingOn == 0)
					return 0;
				var workingOnAsPct = (Double)_workLoadInProgress / _totalWorkLoad;
				var workPct = workingOnAsPct * _progressives.Product(p => p.PercentComplete);
				if (workPct + _percentComplete > 1)
				{ }

				return workPct;
			}
		}
		
		private readonly IDirectSeriesProcessor<TInput> _processor;
		private readonly ConcurrentQueue<TInput> _pendingRecords;
		private readonly ConcurrentDictionary<TInput, Int64> _recordSizes;
		private readonly ConcurrentQueue<ISeriesProcessor> _seriesProcessors;

		private Int32 _totalRecords;

		private Int32 _recordsProcessed;
		private Int32 _recordsHavingStarted;

		public ChainedWorkCoordinator(ITypeFinder typeFinder,
			IPublisherProvider publishers, IWorkerContainer workers,
			IDirectSeriesProcessor<TInput> processor)
			: base(typeFinder, publishers, workers)
		{
			_nullArgs = new EventArgs();
			_processor = processor;
			_progressives = new List<IProgressive>();
			_pendingRecords =new ConcurrentQueue<TInput>();
			_seriesProcessors = new ConcurrentQueue<ISeriesProcessor>();
			_recordSizes = new ConcurrentDictionary<TInput, Int64>();

			publishers.ProgressChanged += OnProgressChanged;

			publishers.Starting += OnStarting;
			publishers.Finishing += (sender, e) => Finishing?.Invoke(sender, e);
			publishers.Finished += OnPublishersFinished; 
		}

		private void OnPublishersFinished(Object sender, EventArgs e)
		{
			if (IsFinished)
				Finished?.Invoke(this, e);
		}

		private void OnStarting(Object sender, EventArgs e)
		{
			_publishers.TryAddSubscriber(this);
			Starting?.Invoke(this, e);
			foreach (var series in _seriesProcessors)
				series.IsStarted = true;
		}

		private void OnProgressChanged(Object sender, Double e) => UpdateCompletion();			
		
		private void UpdateCompletion()
		{
			var pct = _publishers.PercentComplete;

			Double myPct;
			if (_totalWorkLoad > 0)
				myPct = (Double)_completedWorkLoad / _totalWorkLoad;
			else if (_totalRecords > 0)
				myPct = (Double)_recordsProcessed / _totalRecords;
			else myPct = 0;

			pct *= myPct;

			if (pct > _percentComplete)
			{
				_percentComplete = pct;
				ProgressChanged?.Invoke(this, _percentComplete);
			}
		}

		public override void AddWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer)
		    => AddWorker(worker);
		

        public override void AddWorker<TWorkItem>(IWorker<TWorkItem> worker)
        {
            switch (worker)
            {
                case IOneToNoneWorker<TWorkItem> oneToNone:
                    Action<TWorkItem> term = oneToNone.Process;
                    AddTerminator(oneToNone, term);
//
//                    if (!TryAddSubscriber(oneToNone))
//                    {
//                        if (!TryContrivePublisher(oneToNone))
//                            throw new InvalidDataContractException();
//                    }
                    break;
                case IDataSubscriber<TWorkItem> subs:
                    Action<TWorkItem> act = subs.AddData;
                    AddTerminator(worker, act);
                    break;
                default:
                    throw new NotSupportedException();
            }

            _workers.AddWorker(worker);
            TryAddAsProgressive(worker);
        }

        public override void AddWorker<TWorkerInput, TOutput>(
			IRelayWorker<TWorkerInput, TOutput> worker, Int32 maximumBuffer)
		{
			AddWorker(worker);
		}

        public override void AddWorker<TWorkerInput, TOutput>(
            IRelayWorker<TWorkerInput, TOutput> worker)
        {
            switch (worker)
            {
                case IOneToOneWorker<TWorkerInput, TOutput> oneToOne:
                    Func<TWorkerInput, TOutput> funk = oneToOne.Process;
                    _processor.AddRelayToChain(funk);
                    break;
                case IOneToManyWorker<TWorkerInput, TOutput> oneToMany:
                    Func<TWorkerInput, IEnumerable<TOutput>> funke = oneToMany.Process;
                    _processor.AddIteratorToChain(funke);
                    break;
                default:
                    throw new NotSupportedException();
            }

            _workers.AddWorker(worker);
            TryAddAsProgressive(worker);
        }

        private void TryAddAsProgressive(IWorker worker)
		{
			if (worker is IProgressive progressive)
				_progressives.Add(progressive);
		}

		private Boolean TryContrivePublisher<TWorkItem>(
			IDataSubscriber<TWorkItem> worker)
		{
			var emitters = _workers.GetAllWorkers().OfType<
				IDataEmitter<TWorkItem>>().ToArray();
			if (!emitters.Any())
				return false;

			var emitter = emitters.Last();
			ContriveEmitter<TInput, TWorkItem>(emitter, worker);
			return true;
		}

		private void ContriveEmitter<TWorkItem, TOutput>(
			IDataEmitter<TOutput> emitter, IDataSubscriber<TOutput> subscriber)
		{
			switch (emitter)
			{
				case IOneToOneWorker<TWorkItem, TOutput> oneToOne:
					var cnt = new  ContrivedOneToOneSubscribable
						<TWorkItem, TOutput>(oneToOne);
					cnt.AddSubscriber(subscriber);
					AddWorker(cnt);
					break;
				case IOneToManyWorker<TWorkItem, TOutput> oneToMany:
					var cntx = new ContrivedOneToManySubscribable
						<TWorkItem, TOutput>(oneToMany);
					cntx.AddSubscriber(subscriber);
					AddWorker(cntx);
					break;
			}
		}

		public override Boolean TryAddSubscriber<TWorkItem>(
			IDataSubscriber<TWorkItem> subscriber)
        {
            var src = _processor.GetDataSource<TWorkItem>() ?? 
			    _workers.GetDataSource<TWorkItem>();
            if (src == null) 
                return _publishers.TryAddSubscriber(subscriber);

            src.AddSubscriber(subscriber);
            return true;
        }

		private void AddTerminator<TWorkItem>(IWorker<TWorkItem> worker, 
			Action<TWorkItem> addData)
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			if (worker is ISeriesProcessor<TWorkItem> series && series.MaximumBuffer > 0)
			{
				_processor.AddToCappedChainTerminator(series, addData);
				_seriesProcessors.Enqueue(series);
			}
			else
				_processor.AddToChainTerminator(addData);
		}

		public void AddData(TInput record)
		{
			_pendingRecords.Enqueue(record);
			Interlocked.Increment(ref _totalRecords);
		}

		public void AddData(TInput record, Int64 sizeCoefficient)
		{
			_recordSizes.TryAdd(record, sizeCoefficient);
			Interlocked.Add(ref _totalWorkLoad, sizeCoefficient);
			AddData(record);
			
		}

		public Boolean TryProcessNext()
		{
			if (!_pendingRecords.TryDequeue(out var record))
			{
				foreach (var series in _seriesProcessors)
				{
					if (series.TryProcess(null))
						return true; //sweep series as they could have leftover records
				}
				if (IsFinished)
					Finished?.Invoke(this, _nullArgs);
				
				return false;
			}

			if (!_recordSizes.TryGetValue(record, out var weight))
				weight = 1;

			Interlocked.Increment(ref _recordsHavingStarted);
			Interlocked.Add(ref _workLoadInProgress, weight);

			_processor.Process(record);

			Interlocked.Increment(ref _recordsProcessed);
			Interlocked.Add(ref _workLoadInProgress, -weight);
			Interlocked.Add(ref _completedWorkLoad, weight);

			UpdateCompletion();
			return true;
		}

		public override void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
			=> _publishers.AddPublisher(publisher);

		public override void Dispose()
		{
			_processor.Dispose();
			foreach (var series in _seriesProcessors)
				series.Dispose();
		}

		public override String ToString()
		{
			return "Chain work coordinator " + _recordsProcessed + "/"  + _totalRecords;
		}
	}
}
