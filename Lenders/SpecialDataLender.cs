using System;
using System.Collections.Concurrent;
using Das.DataCore;

namespace Das.DataFlow
{
	public class SpecialDataLender<T> : LenderCore<T> where T : ILendable<T>
	{
		private readonly Func<T> _getNew;

		public SpecialDataLender(Func<T> getNew)
		{
			_getNew = getNew;
		}

		protected override T GetInternal() => _getNew();
	}

	public class SpecialDataLender<T, TParam1, TParam2> :
		IDataLender<T, TParam1, TParam2>
		where T : ILendable<T, TParam1, TParam2>
	{
		private readonly Func<TParam1, TParam2, T> _getter;
		private readonly ConcurrentQueue<T> _inventory;

		public SpecialDataLender(Func<TParam1, TParam2, T> getter)
		{
			_getter = getter;
			_inventory = new ConcurrentQueue<T>();
		}

		public T Get(TParam1 input1, TParam2 input2)
		{
			if (!_inventory.TryDequeue(out var res))
			{
				return _getter(input1, input2);
			}

			res.Construct(input1, input2);
			return res;
		}


		public void Put(T item)
		{
			throw new NotImplementedException();
		}
	}

	public class SpecialDataLender<T, TParam> :
	IDataLender<T, TParam> where T : ILendable<T, TParam>
	{
		private readonly Func<TParam, T> _getter;
		private readonly ConcurrentQueue<T> _inventory;

		public SpecialDataLender(Func<TParam, T> getter)
		{
			_getter = getter;
			_inventory = new ConcurrentQueue<T>();
		}

		public T Get(TParam input)
		{
			if (!_inventory.TryDequeue(out var res))
			{
				return _getter(input);
			}

			res.Construct(input);
			return res;
		}


		public void Put(T item)
		{
			throw new NotImplementedException();
		}
	}
}
