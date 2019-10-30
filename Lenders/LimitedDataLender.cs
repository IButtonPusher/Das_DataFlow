using System;
using System.Collections.Concurrent;
using System.Threading;
using Das.DataCore;

namespace Das.DataFlow
{
	public class LimitedDataLender<T> : IDataLender<T> where T : ILendable<T>, new()
	{
		public Int32 MaximumConcurrent { get; }

		private readonly ConcurrentQueue<T> _inventory;
		private readonly ConcurrentQueue<T> _refurbishQueue;
		private Int32 _currentLentOut;

		public LimitedDataLender(Int32 maximumConcurrentToLend)
		{
			MaximumConcurrent = maximumConcurrentToLend;
			_inventory = new ConcurrentQueue<T>();
			_refurbishQueue = new ConcurrentQueue<T>();
		}

		public T Get()
		{
			if (!_inventory.TryDequeue(out var res))
			{
				if (_refurbishQueue.TryDequeue(out res))
				{
					res.Dispose();
					return res;
				}

				if (Interlocked.Increment(ref _currentLentOut) > MaximumConcurrent)
					throw new Exception("Too many items lent out");
				res = new T();
			}

			return res;
		}

		public void Put(T item)
		{
			Interlocked.Decrement(ref _currentLentOut);
			_refurbishQueue.Enqueue(item);
		}
	}
}
