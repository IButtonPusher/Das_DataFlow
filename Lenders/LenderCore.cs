using System;
using System.Collections.Concurrent;
using Das.DataCore;

namespace Das.DataFlow
{
    public abstract class LenderCore<T> : IDataLender<T> where T : ILendable<T>
	{
		private readonly ConcurrentQueue<T> _inventory;		

		protected LenderCore()
		{
			_inventory = new ConcurrentQueue<T>();			
		}

		public virtual T Get()
		{
			if (!_inventory.TryDequeue(out var res))
			{				
				res = GetInternal();
			}

			if (res.LendingState == LendingState.Active)
				throw new Exception("Item is already active");

			res.LendingState = LendingState.Active;
			return res;
		}

		public void Put(T item)
		{

			if (item.LendingState == LendingState.Inventory)
				throw new Exception("Item is already in inventory");
			item.LendingState = LendingState.Inventory;
			_inventory.Enqueue(item);
		}

		protected abstract T GetInternal();
	}
}
