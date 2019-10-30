using System;

namespace Das.DataFlow
{
	public class BlockingInvoker : IActionInvoker
	{
		public Boolean TryInvoke(Action action)
		{
			action();
			return true;
		}

		public void Invoke(Action action) => action();
		public T Get<T>(Func<T> func) => func();
		public Boolean TryGet<T>(Func<T> func, out T res, out Exception failException)
		{
			try
			{
				res = func();
				failException = null;
				return true;
			}
			catch (Exception ex)
			{
				failException = ex;
				res = default;
				return false;
			}
		}

		public Boolean TryInvoke(Action action, out Exception failException)
		{
			try
			{
				action();
				failException = null;
				return true;
			}
			catch (Exception ex)
			{
				failException = ex;
				return false;
			}
		}
	}
}
