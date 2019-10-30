using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Das.DataFlow
{
	/// <summary>
	/// A system that supports single or multi threaded processing.  If using multiple threads
	/// then the final TWorkProducts are very unlikely to be published in an order corresponding
	/// to the original IDataPublisher who provides the input. Each IWorker added has their
	/// "Process(...)" input provided by the output of the previous IWorker.
	/// </summary>
	/// <typeparam name="TWorkProduct"></typeparam>
	internal class ChainedWorkSystem<TWorkProduct> : CoreWorkSystem<TWorkProduct>
	{
		public override Boolean IsFinished => base.IsFinished && _seriesMaster.IsFinished &&
			WorkerProvider.IsFinished;

		private readonly Int32 _threadCount;
		private readonly ConcurrentQueue<Thread> _workerThreads;
		private readonly ISeriesMaster _seriesMaster;

		public ChainedWorkSystem(IWorkerCoordinator workerProvider, ISeriesMaster seriesMaster,
			Int32 threadCount)  : base(workerProvider)
		{
			_seriesMaster = seriesMaster;
			_seriesMaster.ProgressChanged += OnProgressChanged;
			_threadCount = threadCount;
			
			_workerThreads =new ConcurrentQueue<Thread>();
		}

		private void OnProgressChanged(Object sender, Double e)
			=> ProgressChanged?.Invoke(this, e);
		

		public override event EventHandler<Double> ProgressChanged;

		protected override void Run()
		{
			for (var i = 0; i < _threadCount; i++)
			{
				var thread = new Thread(RunThread);
				thread.Start(i);
				_workerThreads.Enqueue(thread);
			}

			while (!WorkerProvider.IsReadyToStart && IsProcessing)
				Thread.Sleep(1);
		}


		private void RunThread(Object index)
		{
			Thread.CurrentThread.IsBackground = true;
			Thread.CurrentThread.Name = "Task thread " + index;
			while (IsProcessing)
			{
				if (!_seriesMaster.TryProcessNext())
					Thread.Sleep(1);
			}

			NotifyFinished();
		}
	}
}
