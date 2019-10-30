using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal class ContrivedOneToManySubscribable<TInput, TOutput>
		: IOneToManyWorker<TInput, TOutput>, IDataSubscribable<TOutput>
	{
		private readonly IOneToManyWorker<TInput, TOutput> _enveloping;
		private readonly List<IDataSubscriber<TOutput>> _subscribers;

		public ContrivedOneToManySubscribable(
			IOneToManyWorker<TInput, TOutput> enveloping)
		{
			_enveloping = enveloping;
			_subscribers = new List<IDataSubscriber<TOutput>>();
		}


		public void AddSubscriber(IDataSubscriber<TOutput> subscriber)
		{
			_subscribers.Add(subscriber);
		}

		public Boolean IsStarted { get; }
		public Boolean IsFinished { get; }


		public IEnumerable<TOutput> Process(TInput input)
		{
			foreach (var res in _enveloping.Process(input))
			{
				foreach (var subs in _subscribers)
					subs.AddData(res);

				yield return res;
			}
		}

		public Boolean IsCancelled { get; set; }

		public void AddSubscriber(IDataSubscriber subscriber) => _subscribers.Add(
			(IDataSubscriber<TOutput>)subscriber);
	}
}
