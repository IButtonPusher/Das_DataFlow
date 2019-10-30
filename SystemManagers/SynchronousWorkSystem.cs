using System;
using System.Threading;

namespace Das.DataFlow
{
	/// <summary>
	/// A single threaded system wherein each worker publishes records until either he
	/// has no more or until one or more of the next workers in the chain's buffer is full.
	/// The single thread loops through the buffered workers until all workers have published all
	/// their records
	/// </summary>
	/// <typeparam name="TWorkProduct"></typeparam>
	internal class SynchronousWorkSystem<TWorkProduct> : CoreWorkSystem<TWorkProduct>
	{
		private readonly ISeriesBuilder _seriesBuilder;

		public override event EventHandler<Double> ProgressChanged;
		public override Boolean IsFinished => base.IsFinished && _seriesBuilder.IsFinished;

		//public override event EventHandler<int> ProgressChanged;

		public SynchronousWorkSystem(IWorkerCoordinator workers,  ISeriesBuilder seriesBuilder)
			:base(workers)
		{
			_seriesBuilder = seriesBuilder;
		}

		protected override void Run()
		{
			Thread.CurrentThread.Name = "task thread";

			while (!WorkerProvider.IsReadyToStart && IsProcessing)
			{
				Thread.Sleep(1);
				if (WorkerProvider.IsFinished)
					break; //odd...
			}

			RunWithData();
		}

		

		private void RunWithData()
		{
			Boolean hadRecord;
			var lastPct = 0;
			
				do
				{
					//this loops breaks when no processor could process a record
					hadRecord = false;

					foreach (var proc in _seriesBuilder.GetTaskedRelays())
						hadRecord |= proc.Item1.TryProcess(proc.Item2);

					var pct = Convert.ToInt32(PercentComplete);
					if (pct > lastPct)
					{
						ProgressChanged?.Invoke(this, pct);
						lastPct = pct;
					}
				}
				while ((hadRecord || WorkerProvider.IsPublishedDataAvailable) && IsProcessing);

				if (IsProcessing)
					CompleteProcessing();
		}

		public override String ToString() => WorkerProvider.ToString();
	}
}
