using System;

namespace Das.DataFlow
{
	public interface ISeriesProcessor<in TWorkItem> : ISeriesProcessor, 
		IWorker<TWorkItem>, IDataSubscriber<TWorkItem>
	{
		void SetDataSource(IDataSubscribable source);
	}

	public interface ISeriesProcessor : IWorker, IDisposable, IDataSubscriber,
		IProgressive
	{
		Boolean HasAvailableTask { get; }

		Boolean HasDataSource { get; }

		Boolean IsActivelyWorking { get; }

		Int32 TotalProcessed { get; }

		Int32 TotalPublished { get; }

		Int32? MaximumBuffer { get; }
		Int32? AvailableBuffer { get; }
		Int32 CurrentBufferSize { get; }

		Boolean IsStarted { get; set; }

		/// <summary>
		/// Process available item in PendingTasksCollection
		/// </summary>
		/// <param name="maxToPublish"></param>
		/// <returns>true if any were processed</returns>
		Boolean TryProcess(Int32? maxToPublish);
	}
}
