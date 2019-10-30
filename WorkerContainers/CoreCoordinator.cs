using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Das.DataFlow
{
    internal abstract class CoreCoordinator : IWorkerCoordinator
	{
		public virtual event EventHandler<Double> ProgressChanged;
		public virtual event EventHandler Starting;
		public virtual event EventHandler Finishing;
		public virtual event EventHandler Finished;
		public event EventHandler Cancelling;

		protected readonly ITypeFinder _typeFinder;
		protected readonly IPublisherProvider _publishers;
		protected readonly IWorkerContainer _workers;

		public Boolean HasWorkers => _workers.HasWorkers;

		public CoreCoordinator(ITypeFinder typeFinder,
			IPublisherProvider publishers, IWorkerContainer workers)
		{
			_typeFinder = typeFinder;
			
			_publishers = publishers;
			_publishers.Starting += (sender, e) =>  Starting?.Invoke(sender, e);
			_publishers.Finishing += (sender, e) => Finishing?.Invoke(sender, e);
			_publishers.Finished += (sender, e) => Finished?.Invoke(sender, e);
			_publishers.ProgressChanged += (sender, e) => ProgressChanged?.Invoke(sender, e);
			_publishers.PublisherAdded += (o, e) => PublisherAdded?.Invoke(o, e);
			_publishers.Cancelling += (o, e) => Cancelling?.Invoke(o, e);

			_workers = workers;
		}

		public virtual Boolean IsReadyToStart => (_publishers.IsPublishedDataAvailable &&
			_workers.HasWorkers);


		public virtual Int32 TotalRecordsPublishing => _publishers.TotalRecordsPublishing;

		public virtual void AssignDataSources(ISeriesProcessor processor)
		{
			if (processor.HasDataSource)
				return;

			var subscribingTo = _typeFinder.GetGenericImplementations<IDataSubscriber>(processor);
			var implementations = _typeFinder.GetImplementations<IDataSubscribable>(
				subscribingTo, GetAll());
			foreach (var impl in implementations)
			{
				impl.AddSubscriber(processor);
			}
		}

		public  virtual Boolean TryAddSubscriber<TInput>(IDataSubscriber<TInput> subscriber)
			=> _publishers.TryAddSubscriber(subscriber);

		protected IEnumerable<IEnumerable> GetAll()
		{
			yield return _publishers.GetAllPublishers();
			yield return _workers.GetAllWorkers();
		}

		public virtual void Start() => _publishers.Start();

		public void Cancel() => _publishers?.Cancel();

		public abstract void AddWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer);
        public abstract void AddWorker<TWorkItem>(IWorker<TWorkItem> worker);

        public abstract void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker, 
            Int32 maximumBuffer);

        public abstract void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker);

        void IWorkerAdder.AddWorker(IWorker worker) => throw new NotSupportedException();

		public virtual IDataEmitter<TOutput> GetWorkerByOutput<TOutput>()
			=> _workers.GetWorkerByOutput<TOutput>();

		public virtual IRelayWorker<TInput, TOutput> GetWorker<TInput, TOutput>()
			=> _workers.GetWorker<TInput, TOutput>();

		public virtual IWorker<TInput> GetWorkerByInput<TInput>()
			=> _workers.GetWorkerByInput<TInput>();

		public virtual IEnumerable<IWorker> GetAllWorkers() => _workers.GetAllWorkers();

		public virtual void Dispose()
		{
			_publishers.Dispose();
			_workers.Dispose();
		}

		public Boolean IsPublishedDataAvailable => _publishers?.IsPublishedDataAvailable == true;

		public abstract Boolean IsStarted { get; }
		public abstract Boolean IsFinished { get; }
		public abstract Boolean IsCancelled { get; }

		public abstract Double PercentComplete { get; }
		public virtual void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
			=> _publishers.AddPublisher(publisher);

		public virtual void AddPublisher(IDataPublisher publisher)
			=>_publishers.AddPublisher(publisher);

		public Int32 PublisherCount => _publishers.PublisherCount;

		public virtual IEnumerable<IDataPublisher> GetAllPublishers()
		{
			foreach (var pub in _publishers.GetAllPublishers())
				yield return pub;

			foreach (var worker in _workers.GetAllWorkers())
			{
				if (worker is IDataPublisher pubo)
					yield return pubo;
			}
		}

		public IEnumerable<IDataSubscribable> GetAllDataSubscribables()
		{
			foreach (var ds in _workers.GetAllWorkers().OfType<IDataSubscribable>())
				yield return ds;
			foreach (var ds in _publishers.GetAllDataSubscribables())
				yield return ds;
		}

		public virtual IDataPublisher<TOutput> GetPublisher<TOutput>()
			=> _publishers.GetPublisher<TOutput>();

		public virtual IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
			=> _publishers.GetDataSource<TWorkItem>();

		public IDataSubscribable GetDataSource(IDataSubscriber forWorker)
			=> _workers.GetDataSource(forWorker) ?? _publishers.GetDataSource(forWorker);

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(
			IEnumerable<IEnumerable> toSearch) => _publishers.GetDataSource<TWorkItem>(
				toSearch);

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(IEnumerable toSearch)
			=> _publishers.GetDataSource<TWorkItem>(toSearch);

		public event EventHandler<IDataPublisher> PublisherAdded;
		IEnumerator<IDataPublisher> IEnumerable<IDataPublisher>.GetEnumerator()
			=> GetAllPublishers().GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator() => GetAllPublishers().GetEnumerator();
	}
}
