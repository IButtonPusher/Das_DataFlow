using System;
using Das.DataFlow.Series;

namespace Das.DataFlow
{
    public class ChainedSystemProvider<TInput> : IWorkSystemProvider
	{
		private readonly Int32 _threadCount;

		public ChainedSystemProvider(Int32 threadCount)
		{
			_threadCount = threadCount;
		}

		public IWorkSystem<TWorkProduct> GetWorkSystem<TWorkProduct>()
		{
			var typeFinder = new TypeFinder();
			var publisherContainer = new CorePublisherContainer(typeFinder);
			var publisherProvider = new CorePublisherProvider(
				publisherContainer);
			var workerContainer = new CoreWorkerContainer(typeFinder);

			var processor = new ChainedProcessor<TInput>(typeFinder);		

			var series = new ChainedWorkCoordinator<TInput>(typeFinder, publisherProvider,
				workerContainer, processor);
			
			return new ChainedWorkSystem<TWorkProduct>(series,
				series, _threadCount);
		}
	}
}
