using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Das.DataFlow
{
    public abstract class CoreSubscribable<TWorkItem> : IDataSubscribable<TWorkItem>
	{
		private readonly List<IDataSubscriber<TWorkItem>> _subscribers;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		protected void SendToSubscribers(TWorkItem item)
		{
			foreach (var subs in _subscribers)
				subs.AddData(item);
		}

		protected CoreSubscribable()
		{
			_subscribers = new List<IDataSubscriber<TWorkItem>>();
		}

		public void AddSubscriber(IDataSubscriber<TWorkItem> subscriber)
		{
			_subscribers.Add(subscriber);
		}

		public virtual Boolean IsStarted { get; }
		public virtual Boolean IsFinished { get; }
		public virtual Boolean IsCancelled { get; }

		public virtual void AddSubscriber(IDataSubscriber subscriber)
		{
			_subscribers.Add((IDataSubscriber<TWorkItem>)subscriber);
		}
	}
}
