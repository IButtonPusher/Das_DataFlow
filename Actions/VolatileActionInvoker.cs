using System;
using System.Threading;

namespace Das.DataFlow
{
	public class VolatileActionInvoker : IActionInvoker
	{
		protected Int32 TotalTries;
        private Int32 _waitOnFail;
		protected Action<String> FailLog;

		public VolatileActionInvoker(Int32 retryCount, Int32 waitOnFail,
            Action<String> failLog)
		{
			if (retryCount < 0)
				throw new ArgumentOutOfRangeException("Retry count cannot be negative");
            _waitOnFail = waitOnFail;
            TotalTries = retryCount + 1;
			FailLog = failLog;
		}
		public Boolean TryInvoke(Action action, out Exception lastEx)
		{
			lastEx = null;

			for (var i = 0; i < TotalTries; i++)
			{
				try
				{
					action();
					lastEx = null;
					return true;
				}
				catch (Exception ex)
				{
					lastEx = ex;
					HandleError(ex);
					Thread.Sleep(_waitOnFail);
				}
			}

			return false;
		}

		public void Invoke(Action action)
		{
			Exception ex = null;

			for (var i = 0; i < TotalTries; i++)
			{
				try
				{
					action();
					return;
				}
				catch (Exception x)
				{
					HandleError(x);
					ex = x;
					Thread.Sleep(_waitOnFail);
				}
			}

			if (ex != null)
				throw ex;
		}

		public T Get<T>(Func<T> func)
		{
			Exception ex = null;

			for (var i = 0; i < TotalTries; i++)
			{
				try
				{
					return func();
				}
				catch (Exception x)
				{
					HandleError(x);
					ex = x;
					Thread.Sleep(_waitOnFail);
				}
			}
			
			throw ex ?? new InvalidOperationException();
		}

		public Boolean TryGet<T>(Func<T> func, out T res, out Exception failException)
		{
			failException = null;

			for (var i = 0; i < TotalTries; i++)
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
					HandleError(ex);
					Thread.Sleep(_waitOnFail);
				}
			}

			res = default;
			return false;
		}

		protected virtual void HandleError(Exception ex)
		{
			FailLog?.Invoke($"Error {ex.Message}: " + ex.Message);
		}
	}
}
