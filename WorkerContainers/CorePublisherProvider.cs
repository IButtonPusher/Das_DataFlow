using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Das.DataFlow
{
	internal class CorePublisherProvider : IPublisherProvider
	{
		public event EventHandler<IDataPublisher> PublisherAdded;

		public event EventHandler Starting;
		public event EventHandler<Double> ProgressChanged;
		public event EventHandler Finishing;
		public event EventHandler Finished;
		public event EventHandler Cancelling;

		public Double PercentComplete => _percentComplete;

		//private readonly ITypeFinder _typeFinder;
		private readonly IPublisherContainer _publisherContainer;

		private readonly ConcurrentDictionary<IDataPublisher, Byte> _finishingPublishers;
		private readonly ConcurrentDictionary<IDataPublisher, Byte> _finishedPublishers;
		private Boolean _isCancelled;

		//private readonly List<IEnumerable> _collectionSearch;

		public CorePublisherProvider(IPublisherContainer publisherContainer)
		{
			//_typeFinder = typeFinder;
			_publisherContainer = publisherContainer;
			_publisherContainer.PublisherAdded += OnPublisherAdded;

			_finishingPublishers = new ConcurrentDictionary<IDataPublisher, Byte>();
			_finishedPublishers = new ConcurrentDictionary<IDataPublisher, Byte>();
		}

		private void OnPublisherAdded(Object sender, IDataPublisher e)
		{
			e.ProgressChanged += OnPublisherProgressChanged;
			e.Cancelling += OnPublisherCancelling;
			PublisherAdded?.Invoke(sender, e);
		}

		private void OnPublisherCancelling(Object sender, EventArgs e)
		{
			_isCancelled = true;
			Cancelling?.Invoke(this, e);
		}

		private void OnPublisherProgressChanged(Object sender, Double e)
		{
			UpdateCompletion();
			ProgressChanged?.Invoke(this, PercentComplete);
		}

		private void UpdateCompletion()
		{
			var pct = 1.0;
			foreach (var pub in _publisherContainer.GetAllPublishers())
				pct *= pub.PercentComplete;

			_percentComplete = pct;
		}
		//public event EventHandler<int> ProgressChanged;


		public virtual void Dispose()
		{

		}

		//		private void OnPublisher

		public Boolean IsPublishedDataAvailable => _publisherContainer.GetAllPublishers()
			.Any(p => !p.IsFinished);

		public Int32 TotalRecordsPublishing => _publisherContainer.TotalRecordsPublishing;

		private Int32 _started;
		private Double _percentComplete;

		public Boolean IsStarted => _started == 1;
		public Boolean IsFinished { get; protected set; }
		public Boolean IsCancelled => _isCancelled;

		public virtual void AssignDataSources(ISeriesProcessor processor)
		{
			var dataSource = _publisherContainer.GetDataSource(processor);
			dataSource.AddSubscriber(processor);

			//			var subscribingTo = _typeFinder.GetGenericImplementations<IDataSubscriber>(processor);
			//			var implementations = _typeFinder.GetImplementations<IDataSubscribable>(subscribingTo,
			//				
			//			foreach (var impl in implementations)
			//				impl.AddSubscriber(processor);
		}

		public Boolean TryAddSubscriber<TInput>(IDataSubscriber<TInput> subscriber)
		{
			var source = _publisherContainer.GetDataSource<TInput>();
			if (source == null)
				return false;

			source.AddSubscriber(subscriber);
			return true;
		}


		public void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
		{
			AddLocal(publisher);
			_publisherContainer.AddPublisher(publisher);
		}

		public void AddPublisher(IDataPublisher publisher)
		{
			AddLocal(publisher);
			_publisherContainer.AddPublisher(publisher);
		}

		private void AddLocal(IDataPublisher publisher)
		{
			publisher.Starting += OnPublisherStarted;
			publisher.Finishing += OnPublisherFinishing;
			publisher.Finished += OnPublisherFinished;
		}

		private void OnPublisherFinished(Object sender, EventArgs e)
		{
			if (sender is IDataPublisher publisher)
			{
				_finishedPublishers.TryAdd(publisher, 1);
				if (_finishingPublishers.Count == _publisherContainer.PublisherCount)
				{
					IsFinished = true;
					Finished?.Invoke(this, e);
				}

			}
		}

		private void OnPublisherStarted(Object sender, EventArgs e)
		{
			MaybeStarted();
		}

		private void OnPublisherFinishing(Object sender, EventArgs e)
		{
			if (sender is IDataPublisher publisher)
			{
				_finishingPublishers.TryAdd(publisher, 1);
				if (_finishingPublishers.Count == _publisherContainer.PublisherCount)
					Finishing?.Invoke(this, e);
			}
		}

		public Int32 PublisherCount => _publisherContainer.PublisherCount;

		public IEnumerable<IDataPublisher> GetAllPublishers()
			=> _publisherContainer.GetAllPublishers();

		public IEnumerable<IDataSubscribable> GetAllDataSubscribables()
			=> _publisherContainer.GetAllDataSubscribables();

		public IDataPublisher<TOutput> GetPublisher<TOutput>()
			=> _publisherContainer.GetPublisher<TOutput>();

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
			=> _publisherContainer.GetDataSource<TWorkItem>();

		public IDataSubscribable GetDataSource(IDataSubscriber forWorker)
			=> _publisherContainer.GetDataSource(forWorker);

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(
			IEnumerable<IEnumerable> toSearch)
				=> _publisherContainer.GetDataSource<TWorkItem>(toSearch);

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(IEnumerable toSearch)
			=> _publisherContainer.GetDataSource<TWorkItem>(toSearch);


		public void Start()
		{
			foreach (var pub in _publisherContainer.GetAllPublishers())
				pub.Start();
			MaybeStarted();

		}

		private void MaybeStarted()
		{
			if (Interlocked.Exchange(ref _started, 1) == 0)
				Starting?.Invoke(this, new EventArgs());
		}

		public void Cancel()
		{
			_isCancelled = true;
			foreach (var pub in _publisherContainer.GetAllPublishers())
				pub.Cancel();
		}
		

		IEnumerator<IDataPublisher> IEnumerable<IDataPublisher>.GetEnumerator()
			=> GetAllPublishers().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetAllPublishers().GetEnumerator();
	}
}
