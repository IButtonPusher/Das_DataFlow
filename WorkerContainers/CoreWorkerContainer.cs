using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Das.DataFlow
{
    internal class CoreWorkerContainer : IWorkerContainer
    {
	    private readonly ITypeFinder _typeFinder;
        private readonly BlockingCollection<IWorker> _workers;

	    public CoreWorkerContainer(ITypeFinder typeFinder)
	    {
		    _typeFinder = typeFinder;
		    _workers = new BlockingCollection<IWorker> ();
	    }

	    public void AddWorker<TWorkItem>(IWorker<TWorkItem> worker, 
			Int32 maximumBuffer) => _workers.Add(worker);

        public void AddWorker<TWorkItem>(IWorker<TWorkItem> worker)
            => _workers.Add(worker);

        public void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker, 
			Int32 maximumBuffer) => _workers.Add(worker);

        public void AddWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker)
            => _workers.Add(worker);

        public void AddWorker(IWorker worker) => _workers.Add(worker);

		public Boolean HasWorkers => _workers?.Any() == true;

	    public IDataEmitter<TOutput> GetWorkerByOutput<TOutput>()
	    {
		    throw new NotImplementedException();
	    }

	    public IRelayWorker<TInput, TOutput> GetWorker<TInput, TOutput>()
	    {
		    throw new NotImplementedException();
	    }

	    public IWorker<TInput> GetWorkerByInput<TInput>()
	    {
		    throw new NotImplementedException();
	    }

	    public void Dispose()
	    {
		    
	    }

		public IEnumerable<IWorker> GetAllWorkers() => _workers;


	    public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
		    => _typeFinder.GetGenericImplementation
			<IDataSubscribable, IDataSubscribable<TWorkItem>>(GetAllWorkers());

		public IDataSubscribable GetDataSource(IDataSubscriber forWorker)
			=> _typeFinder.GetImplementationWithMatchingConstraints
				<IDataSubscriber, IDataSubscribable>(forWorker, GetAllWorkers());

		
    }
}
