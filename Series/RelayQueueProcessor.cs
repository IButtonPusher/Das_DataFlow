using System;

namespace Das.DataFlow
{
	internal class RelayQueueProcessor<TInput, TOutput> 
		: SeriesProcessor<TInput>,
			IRelayProcessor<TInput, TOutput>, ISeriesProcessor
	{
		private readonly IRelayAgent<TInput, TOutput> _agent;

		public RelayQueueProcessor(IRelayAgent<TInput, TOutput> agent,
			IBufferManager<TInput> bufferManager, IProgressive progressReporter,
			IDataSubscribable<TInput> input) 
			: base(agent, progressReporter, bufferManager, input)
		{
			_agent = agent;
		}

		public Boolean IsCancelled { get; set; }
		

		public Boolean IsStarted => DataInput.IsStarted;

		public override Boolean IsFinished => DataInput?.IsFinished != false &&
			!BufferManager.HasAvailableTask && !WorkManager.IsProcessingWorkItem;

		public void AddSubscriber(IDataSubscriber subscriber)
		{
			if (subscriber is IDataSubscriber<TOutput> tSub)
				_agent.AddSubscriber(tSub);
			else
				throw new Exception(subscriber + " cannot subscribe to " + GetType().Name);
		}

		public void AddSubscriber(IDataSubscriber<TOutput> subscriber)
			=> _agent.AddSubscriber(subscriber);

		public void PublishData(TOutput data) //where TData : TOutput
		{
			_agent.PublishData(data);
		}
	}
}
