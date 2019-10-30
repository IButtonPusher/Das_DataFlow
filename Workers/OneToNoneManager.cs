using DataFlow.Interfaces;
using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
	public class OneToNoneManager<TInput> : IWorkManager<TInput>
	{
		public Boolean IsProcessingWorkItem { get; protected set; }
		public Type WorkerType => _worker.GetType();

		private readonly IOneToNoneWorker<TInput> _worker;

		public OneToNoneManager(IOneToNoneWorker<TInput> worker)
		{
			_worker = worker;
		}

		public void Dispose()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			(_worker as IDisposable)?.Dispose();
		}

		public Int32 ProcessNext(TInput input, Int32? maxToDistribute)
		{
			IsProcessingWorkItem = true;
			try
			{
				_worker.AddData(input);
				return 1;
			}
			finally { IsProcessingWorkItem = false; }
		}

		public Int32 ProcessItems(IEnumerable<TInput> inputs)
		{
			IsProcessingWorkItem = true;
			var returning = 0;
			try
			{
				foreach (var input in inputs)
				{
					_worker.AddData(input);
					returning++;
				}

				return returning;
			}
			finally { IsProcessingWorkItem = false; }
		}


		public override String ToString()
		{
			return "1-0 " + _worker.GetType().Name + "(" + typeof(TInput).Name + ")";
		}
	}
}
