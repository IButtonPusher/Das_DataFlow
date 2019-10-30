
namespace Das.DataFlow
{
    public class SynchronousSystemProvider : IWorkSystemProvider
    {
		public IWorkSystem<TWorkProduct> GetWorkSystem<TWorkProduct>()
		{
			var typeFinder = new TypeFinder();

			var publisherContainer = new CorePublisherContainer(typeFinder);
			var workerContainer = new CoreWorkerContainer(typeFinder);

			var series = new QueuedSeriesBuilder(publisherContainer);//, workerContainer);
			var publisherProvider = new CorePublisherProvider(//typeFinder,
				publisherContainer);

			var sources = new SynchronousWorkCoordinator(typeFinder, series,
				publisherProvider, workerContainer);
			

			return new SynchronousWorkSystem<TWorkProduct>(sources, series);
		}
    }
}
