using System;
using System.Collections.Generic;
using System.Text;

namespace Das.DataFlow
{
    internal class SynchronousWorkCoordinator : CoreCoordinator, IWorkerCoordinator
	{
		public override event EventHandler<Double> ProgressChanged;
		private readonly ISeriesBuilder _seriesBuilder;

		public override Boolean IsReadyToStart => _publishers.IsPublishedDataAvailable &&
			(_workers.HasWorkers || _seriesBuilder.HasRelays);

		public SynchronousWorkCoordinator(ITypeFinder typeFinder, ISeriesBuilder seriesBuilder,
			IPublisherProvider publishers, IWorkerContainer workers)
			: base(typeFinder, publishers, workers)
		{
			
			_seriesBuilder = seriesBuilder;
			
			publishers.ProgressChanged += (sender, e) => ProgressChanged?.Invoke(sender, e);
			publishers.Starting += OnStarting;
			
		}

		private void OnStarting(Object sender, EventArgs e)
		{
			foreach (var proc in _seriesBuilder.GetAllProcessors())
				proc.IsStarted = true;
		}


		public override Boolean IsStarted => _publishers.IsStarted;

		public override Boolean IsFinished => !IsPublishedDataAvailable && 
			_seriesBuilder.IsFinished && _publishers.IsFinished;

		public override Boolean IsCancelled => _publishers.IsCancelled;


		public override Double PercentComplete
		{
			get
			{
				if (IsFinished)
					return 100;

				Double res = 100;
				foreach (var pub in _publishers.GetAllPublishers())
					res *= pub.PercentComplete;

				res *= _seriesBuilder.PercentComplete;

//				if (RelayProcesses?.Any() != true)
//					return 0;
//
//				
//				for (var i = 0; i < RelayProcesses.Count; i++)
//					res *= RelayProcesses[i].PercentComplete;
				
				return res;
			}
		}

		

		

//		public override void AddRelay(ISeriesProcessor series)
//		{
//			RelayProcesses.Add(series);
//		}

		
		
//		public IDataPublisher<TOutput> GetDataPublisher<TOutput>()
//		{
//			return _publishers.GetDataSource<TOutput>();
//
//			return Publishers.LastOrDefault(p => p is IDataPublisher<TOutput>)
//				as IDataPublisher<TOutput>;
//		}
		
//		public void AssignDataSources(ISeriesProcessor processor)
//		{
//			var subscribingTo = _typeFinder.GetGenericImplementations<IDataSubscriber>(processor);
//			var implementations = _typeFinder.GetImplementations<IDataSubscribable>(subscribingTo,
//				_collectionSearch);
//			foreach (var impl in implementations)
//				impl.AddSubscriber(processor);
//		}

		//public override void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
			//=> _publishers.AddPublisher(publisher);
		

		//public override void AddPublisher(IDataPublisher publisher) => _publishers.AddPublisher(publisher);


		public override IEnumerable<IDataPublisher> GetAllPublishers() => _publishers.GetAllPublishers();


		

		public override IDataPublisher<TOutput> GetPublisher<TOutput>() => _publishers.GetPublisher<TOutput>();
		
		public override IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
		{
			var tWork = typeof(TWorkItem);
			var imp = _typeFinder.GetImplementation<IDataSubscribable>(tWork, GetAll());
			return imp as IDataSubscribable<TWorkItem>;
		}

		

		public override void Dispose()
		{
			_seriesBuilder.Dispose();
			//DisposeWorkers(Publishers, RelayProcesses);
		}
		

//		private void DisposeWorkers(params IEnumerable[] lists)
//		{
//			foreach (var list in lists)
//			{
//				foreach (var item in list)
//				{
//					var dis = item as IDisposable;
//					dis?.Dispose();
//				}
//			}
//		}

		//public IEnumerator<IDataPublisher> GetEnumerator() => GetAllPublishers().GetEnumerator();
		
		public override String ToString()
		{
			var sb = new StringBuilder(PercentComplete + "% ");
			foreach (var p in _publishers.GetAllPublishers())
				sb.AppendLine(p.GetType().Name + "Published: " + p.TotalPublished + 
					" = " + p.PercentComplete + "%");

			sb.Append(_seriesBuilder);

			
			return sb.ToString();
		}

		public override void AddWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer)
		{
			var res = _seriesBuilder.GetSeriesWorker(worker, maximumBuffer);
            AddSeries<TWorkItem>(res);
		}

        public override void AddWorker<TWorkItem>(IWorker<TWorkItem> worker)
        {
            var res = _seriesBuilder.GetSeriesWorker(worker);
            AddSeries<TWorkItem>(res);
        }

        private void AddSeries<TWorkItem>(IWorker addToSeries)
        {
            switch (addToSeries)
            {
                case IDataPublisher<TWorkItem> publisher:
                    AddPublisher(publisher);
                    break;
                case ISeriesProcessor newSeries:
                    AssignDataSources(newSeries);
                    _seriesBuilder.AddRelay(newSeries);
                    break;
                case NullWorker _:
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

       

        public override void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker,
            Int32 maximumBuffer)
		{
			var series = _seriesBuilder.GetSeriesWorker(worker, maximumBuffer);
			_seriesBuilder.AddRelay(series);
		}

        public override void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker)
        {
            var series = _seriesBuilder.GetSeriesWorker(worker);
            _seriesBuilder.AddRelay(series);
        }
    }
}
