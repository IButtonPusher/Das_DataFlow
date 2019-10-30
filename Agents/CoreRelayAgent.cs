using System;

namespace Das.DataFlow
{
	internal abstract class CoreRelayAgent<TInput, TOutput> : CoreAgent<TOutput>, IDisposable
    {
		public Boolean IsProcessingWorkItem { get; protected set; }
		private readonly IRelayWorker<TInput, TOutput> _workman;

		protected CoreRelayAgent(IRelayWorker<TInput, TOutput> worker)
		{
			_workman = worker;
		}



		public void Dispose()
		{
			// ReSharper disable once SuspiciousTypeConversion.Global
			(_workman as IDisposable)?.Dispose();
		}
	}
}
