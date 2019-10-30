using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal class OneToManyAgent<TInput, TOutput> : CoreRelayAgent<TInput, TOutput>,
		IOneToManyAgent<TInput, TOutput>
	{
		protected IOneToManyWorker<TInput, TOutput> Worker;
		protected ConcurrentQueue<TOutput> SendBuffer;

		public Type WorkerType => Worker.GetType();

		public OneToManyAgent(IOneToManyWorker<TInput, TOutput> worker) : base(worker)
		{
			Worker = worker;
			SendBuffer = new ConcurrentQueue<TOutput>();
		}

		public Int32 Distribute(IEnumerable<TOutput> item)
		{
			var cnt = 0;

			while (SendBuffer.TryDequeue(out var result))
			{
				Distribute(result);
				cnt++;
			}

			foreach (var result in item)
			{
				Distribute(result);
				cnt++;
			}

			return cnt;
		}

		public Int32 Distribute(IEnumerable<TOutput> item, Int32 maxToPublish)
		{
			var cnt = 0;
			while (SendBuffer.TryDequeue(out var result) && cnt < maxToPublish)
			{
				Distribute(result);
				cnt++;
			}

			foreach (var result in item)
			{
				if (cnt < maxToPublish)
				{
					Distribute(result);
					cnt++;
				}
				else
					SendBuffer.Enqueue(result);
			}

			return cnt;
		}

		public Int32 ProcessNext(TInput input, Int32? maxToDistribute)
		{
			IsProcessingWorkItem = true;
			try
			{
				var res = Worker.Process(input);
				return maxToDistribute == null ? Distribute(res) 
					: Distribute(res, maxToDistribute.Value);
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
					returning += Distribute(res);
				}

				return returning;
			}
			finally { IsProcessingWorkItem = false; }
		}

		public override String ToString()
		{
			return "1-N - worker: " + Worker.GetType().Name + "(" + typeof(TInput).Name + "->" +
			       typeof(TOutput).Name + ")";
		}

		public void PublishData(TOutput data)
		{
			throw new NotImplementedException();
		}
	}
}
