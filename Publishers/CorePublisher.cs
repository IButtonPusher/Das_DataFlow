using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Das.DataFlow
{
	public abstract class CorePublisher<T> : IDataPublisher<T>
	{
		public abstract event EventHandler<Double> ProgressChanged;

		public abstract Double PercentComplete { get; }

		public abstract Int32 TotalRecordsPublishing { get; }

		public Int32 TotalRecordsAvailable => TotalRecordsPublishing;

		public Int32 TotalPublished { get; protected set; }

		public Boolean IsCancelled { get; protected set; }

		private readonly HashSet<IDataSubscriber<T>> _subscriberBuilder;

		private IDataSubscriber<T>[] _subscribers;
		private readonly List<IWeightedDataSubscriber<T>>  _weightedSubscribers;


		private readonly Object _subscriberLock;

		public abstract Boolean IsStarted { get; protected set; }
		protected IEnumerable<IDataSubscriber<T>> Subscribers => _subscribers;

		protected void PublishData(T data)
		{
			TotalPublished++;
			foreach (var s in Subscribers)
				s.AddData(data);
		}

		/// <summary>
		/// Sends to IWeightedSubscribers only
		/// </summary>
		/// <param name="data"></param>
		/// <param name="coefficient"></param>
		protected void PublishData(T data, Int64 coefficient)
		{
			TotalPublished++;
			foreach (var s in _weightedSubscribers)
				s.AddData(data, coefficient);
		}

		private Boolean _isFinished;

		public virtual Boolean IsFinished
		{
			get => _isFinished;
			protected set
			{
                if (!value) 
                    return;

                var rdrr = new EventArgs();
                Finishing?.Invoke(this, rdrr);
                _isFinished = true;
                Finished?.Invoke(this, rdrr);
            }
		}
		
		
		public virtual event EventHandler Starting;
		public virtual event EventHandler Finishing;
		public virtual event EventHandler Finished;
		public event EventHandler Cancelling;
		protected readonly Object LockStart;

		protected CorePublisher()
		{
			_weightedSubscribers = new List<IWeightedDataSubscriber<T>>();
			_subscriberLock = new Object();
			LockStart = new Object();
			_subscriberBuilder = new HashSet<IDataSubscriber<T>>();
			_subscribers = _subscriberBuilder.ToArray(); 
		}

		public void AddSubscriber(IDataSubscriber<T> subscriber)
		{
			lock (_subscriberLock)
			{
				switch (subscriber)
				{
					case IWeightedDataSubscriber<T> weighted:
						_weightedSubscribers.Add(weighted);
                        goto default;

					default:
						if (!_subscriberBuilder.Add(subscriber))
							throw new Exception(subscriber + " was already a subscriber");
						
                        _subscribers = _subscriberBuilder.ToArray();
						break;
				}
			}
		}

		public void AddSubscriber(IDataSubscriber subscriber)
			=> AddSubscriber(subscriber as IDataSubscriber<T>);

		public void Cancel()
		{
			IsCancelled = true;
			Cancelling?.Invoke(this, new EventArgs());
		}

		public void Start()
		{
			lock (LockStart)
			{
				if (IsStarted || IsCancelled)
					return;
				IsStarted = true;
			}

			Starting?.Invoke(this, new EventArgs());
			Task.Run(StartPublishing);			
		}

		protected abstract void StartPublishing();

		public abstract void Dispose();
		public abstract IEnumerator<T> GetEnumerator();
		

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
