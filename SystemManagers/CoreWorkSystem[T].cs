using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal abstract class CoreWorkSystem<TWorkProduct> : CoreWorkSystem,
		IWorkSystem<TWorkProduct>
	{
		private readonly ConcurrentQueue<IDataSubscriber<TWorkProduct>> _productSubscribers;

		protected CoreWorkSystem(IWorkerCoordinator workerProvider)
			: base(workerProvider)
		{
			_productSubscribers = new ConcurrentQueue<IDataSubscriber<TWorkProduct>>();
		}
		

		IEnumerator<TWorkProduct> IEnumerable<TWorkProduct>.GetEnumerator()
			=> ((IEnumerable<TWorkProduct>)this).GetEnumerator();

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotSupportedException();
		}

		public override void AddSubscriber(IDataSubscriber subscriber)
		{
			if (TrySubscribeToWorker(subscriber))
				return;

			if (subscriber is IDataSubscriber<TWorkProduct> twp)
				_productSubscribers.Enqueue(twp);
			else
				throw new Exception("Unable to add " + subscriber + " as a subscriber");
		}

		public void AddSubscriber(IDataSubscriber<TWorkProduct> subscriber)
		{
			if (TrySubscribeToWorker(subscriber))
				return;
			_productSubscribers.Enqueue(subscriber);
		}

		private Boolean TrySubscribeToWorker(IDataSubscriber subscriber)
		{
			var source = WorkerProvider.GetDataSource(subscriber);
			if (source != null)
			{
				source.AddSubscriber(subscriber);
				return true;
			}
			return false;
		}

		private class ForEnumerator : IDataSubscriber<TWorkProduct>,
			IEnumerable<TWorkProduct>
		{
			private readonly List<TWorkProduct> _results;

			public ForEnumerator()
			{
				_results = new List<TWorkProduct>();
			}

			public void AddData(TWorkProduct record)
			{
				_results.Add(record);
			}

			public void AddData(TWorkProduct record, Int64 sizeCoefficient)
			{
				_results.Add(record);
			}

			public IEnumerator<TWorkProduct> GetEnumerator() => _results.GetEnumerator();

			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}

		public void PublishData(TWorkProduct data)
		{
			foreach (var sub in _productSubscribers)
				sub.AddData(data);
		}

		public override IDataPublisher<TOutput> GetPublisher<TOutput>()
		{
			if (this is IDataPublisher<TOutput> publisher)
				return publisher;

			return WorkerProvider.GetPublisher<TOutput>();
		}

		
	}
}
