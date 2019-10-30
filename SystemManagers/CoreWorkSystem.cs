using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Das.DataFlow
{
	internal abstract class CoreWorkSystem : IWorkSystem
	{
		public event EventHandler Starting;
		public abstract event EventHandler<Double> ProgressChanged;
		public event EventHandler Finishing;

		public event EventHandler Finished;
		public event EventHandler Cancelling;
		private Int32 _finishedNotified;

		protected Boolean IsProcessing => !IsFinished && !IsCancelled;

		protected readonly IWorkerCoordinator WorkerProvider;
		private readonly Object _lockStart;
		private Boolean _isStarted;

		protected CoreWorkSystem(IWorkerCoordinator workerProvider)
		{
			WorkerProvider = workerProvider;
			WorkerProvider.Starting += OnWorkersStarting;
			WorkerProvider.Finishing += (sender, e) => Finishing?.Invoke(sender, e);
			WorkerProvider.Finished += OnWorkersFinished; 
			WorkerProvider.Cancelling += (o, e) => Cancelling?.Invoke(o, e);
			_lockStart = new Object();
			
		}

		private void OnWorkersFinished(Object sender, EventArgs e)
		{
			NotifyFinished();
		}

		private void OnWorkersStarting(Object sender, EventArgs e)
		{
			IsStarted = true;
			Starting?.Invoke(this, e);
		}

		public Boolean IsStarted { get; protected set; }

		public virtual Boolean IsFinished => WorkerProvider.IsFinished;

		public Double PercentComplete
		{
			get
			{
				var res = WorkerProvider.PercentComplete;
				if (Convert.ToInt32(res) == 100 && !IsFinished)
					res = 99; //not 100 till ProcessCompleting runs

				return res;
			}
		}

		public Boolean IsCancelled { get; set; }

		public Int32 TotalRecordsPublishing => WorkerProvider.TotalRecordsPublishing;

		public Int32 TotalPublished { get; protected set; }

		public void AddWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer)
		{
            AddPublisherOrWorker(worker, () => WorkerProvider.AddWorker(worker, maximumBuffer));
		}

        public void AddWorker<TWorkItem>(IWorker<TWorkItem> worker)
        {
            AddPublisherOrWorker(worker, () => WorkerProvider.AddWorker(worker));
        }

        private void AddPublisherOrWorker<TWorkItem>(IWorker<TWorkItem> worker,
            Action addWorker)
        {
            lock (_lockStart)
            {
                if (worker is IDataPublisher<TWorkItem> publisher)
                {
                    AddPublisher(publisher);
                    return;
                }

                addWorker();
            }
        }

        public void AddWorker<TInput, TOutput>(
			IRelayWorker<TInput, TOutput> worker, Int32 maximumBuffer)
		{
			lock (_lockStart)
				WorkerProvider.AddWorker(worker, maximumBuffer);
			
		}

        public void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker)
        {
            lock (_lockStart)
                WorkerProvider.AddWorker(worker);
        }

        public void AddWorker(IWorker worker)
		{
			lock (_lockStart)
				WorkerProvider.AddWorker(worker);
		}

		public void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
		{
			publisher.Starting += OnProcessorStart;
			WorkerProvider.AddPublisher(publisher);
		}

		public void AddPublisher(IDataPublisher publisher)
		{
			publisher.Starting += OnProcessorStart;
			WorkerProvider.AddPublisher(publisher);
		}

		public IEnumerable<IDataPublisher> GetAllPublishers() => WorkerProvider.GetAllPublishers();
		

		private void OnProcessorStart(Object sender, EventArgs e)
		{
			lock (_lockStart)
			{
				if (_isStarted || IsCancelled)
					return;
			}
			Start();
		}

		public virtual void Cancel()
		{
			if (IsCancelled)
				return;

			Cancelling?.Invoke(this, new EventArgs());
			WorkerProvider.Cancel();
			IsCancelled = true;
		}

		public void Start()
		{
			lock (_lockStart)
			{
				if (_isStarted)
					throw new Exception("Work System already started");
				_isStarted = true;

				if (IsFinished)
					return;

				WorkerProvider.Start();

				Task.Factory.StartNew(Run);
			}
		}

		protected void CompleteProcessing()
		{
			Finishing?.Invoke(this, new EventArgs());
			WorkerProvider.Dispose();
			Finished?.Invoke(this, new EventArgs());
		}

		protected abstract void Run();

		public Int32 TotalRecordsAvailable => WorkerProvider.TotalRecordsPublishing;

		public virtual void AddSubscriber(IDataSubscriber subscriber)
		{
			var subscribeTo = WorkerProvider.GetDataSource(subscriber);
            subscribeTo?.AddSubscriber(subscriber);
        }



		protected void NotifyFinished()
		{
			if (Interlocked.Increment(ref _finishedNotified) == 1)
				Finished?.Invoke(this, new EventArgs());
		}

		IEnumerator IEnumerable.GetEnumerator()
			=> WorkerProvider.GetAllPublishers().GetEnumerator();

		public virtual IDataPublisher<TOutput> GetPublisher<TOutput>()
		{
			if (this is IDataPublisher<TOutput> publisher)
				return publisher;

			var workPublisher = WorkerProvider.GetPublisher<TOutput>();
			if (workPublisher != null)
				return workPublisher;

			var subs = GetDataSource<TOutput>();
			if (subs == null)
				return null;
			return new ContrivedPublisher<TOutput>(this, this, subs);
		}

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
			=> WorkerProvider.GetDataSource<TWorkItem>();
	}

	
}
