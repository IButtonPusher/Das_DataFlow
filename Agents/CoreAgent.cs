using System.Collections.Generic;

namespace Das.DataFlow
{
	internal class CoreAgent<TOutput>
    {
		public IReadOnlyList<IDataSubscriber<TOutput>> Subscribers => _subscribers.AsReadOnly();
		private readonly List<IDataSubscriber<TOutput>> _subscribers;

		public CoreAgent()
		{
			_subscribers = new List<IDataSubscriber<TOutput>>();
		}

		public void Distribute(TOutput item)
		{
			for (var i = 0; i < _subscribers.Count; i++)
			{
				var sub = _subscribers[i];
				sub?.AddData(item);
			}
		}

		public void AddSubscriber<TWorkItem>(IDataSubscriber<TWorkItem> subscriber)
			where TWorkItem : TOutput
		{
			_subscribers.Add((IDataSubscriber < TOutput > )subscriber);
		}
	}
}
