using System;
using System.Collections.Generic;


namespace Das.DataFlow
{
	internal class OneToOneAgent<TInput, TOutput> : CoreRelayAgent<TInput, TOutput>, 
		IOneToOneAgent<TInput, TOutput>		
	{		
		protected IOneToOneWorker<TInput, TOutput> Worker;
		public Type WorkerType => Worker.GetType();

		public OneToOneAgent(IOneToOneWorker<TInput, TOutput> worker) : base(worker)
		{
			Worker = worker;
		}

		public Int32 ProcessNext(TInput input, Int32? maxToDistribute)
		{
			IsProcessingWorkItem = true;
			try
			{
				var res = Worker.Process(input);
				if (res == null)
					return 0;
				Distribute(res);
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
					var res = Worker.Process(input);
					if (res == null)
						continue;
					Distribute(res);
					returning++;
				}

				return returning;
			}
			finally { IsProcessingWorkItem = false; }
		}

		public void PublishData(TOutput data)
		{
			Distribute(data);
		}

		public override String ToString()
		{
			return "1-1 " + Worker.GetType().Name + "(" + typeof(TInput).Name + "->" + 
				typeof(TOutput).Name + ")";
		}
	}
}
