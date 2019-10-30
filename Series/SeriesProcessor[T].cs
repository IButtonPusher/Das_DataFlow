using System;
using System.Linq;
using DataFlow.Interfaces;

namespace Das.DataFlow
{
	internal class SeriesProcessor<TWorkItem> : SeriesProcessor, 
		ISeriesProcessor<TWorkItem>
	{
		protected readonly IWorkManager<TWorkItem> WorkManager;
		protected readonly IBufferManager<TWorkItem> BufferManager;
		protected IDataSubscribable<TWorkItem> DataInput;

		public virtual Type TypeProvidingData => DataInput.GetType();

		public SeriesProcessor(IWorkManager<TWorkItem> workManager,
			IProgressive progressReporter,
			IBufferManager<TWorkItem> bufferManager, IDataSubscribable<TWorkItem> input)
			: base(progressReporter, bufferManager, workManager)
		{
			WorkManager = workManager;

			BufferManager = bufferManager;
			DataInput = input;
			DataInput?.AddSubscriber(this);
		}

		public override Boolean IsFinished => !BufferManager.HasAvailableTask;

		public override Boolean HasDataSource => DataInput != null;

		protected override Boolean TryProcessAll()
		{
			var published = 0;
			while (BufferManager.TryGetNext(out var item))
			{
				published += WorkManager.ProcessNext(item, null);
				TotalProcessed++;
			}

			return FinishWork(published);
		}

		protected override Boolean TryProcessSome(Int32 maxToPublish)
		{
			var published = 0;
			while (published + 1 < maxToPublish && BufferManager.TryGetNext(out var item))
			{
				published += WorkManager.ProcessNext(item, maxToPublish - published);
				TotalProcessed++;
			}

			return FinishWork(published);
		}

		public void SetDataSource(IDataSubscribable source)
		{
			DataInput = (IDataSubscribable<TWorkItem>)source;
			DataInput.AddSubscriber(this);
		}

		public void AddData(TWorkItem record)
		{
			BufferManager.TryAddItem(record);
		}

		public void AddData(TWorkItem record, Int64 sizeCoefficient)
			=> AddData(record);

		public override String ToString()
		{
			return GetType().Name + " mgr: " +  WorkManager + " " + 
				_progressReporter + " " + BufferManager +
				   " Input from " + 
				   DataInput.GetType().
				   GenericTypeArguments?.FirstOrDefault()?.Name;
		}
	}
}
