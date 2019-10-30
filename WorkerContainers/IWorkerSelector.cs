using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    public interface IWorkerSelector
    {
	    Boolean HasWorkers { get; }

		/// <summary>
		/// Gets a worker only in terms of its output
		/// </summary>
		/// <typeparam name="TOutput"></typeparam>
		/// <returns></returns>
		IDataEmitter<TOutput> GetWorkerByOutput<TOutput>();

	    IRelayWorker<TInput, TOutput> GetWorker<TInput, TOutput>();

		IWorker<TInput> GetWorkerByInput<TInput>();

		IEnumerable<IWorker> GetAllWorkers();

		//IDataSubscribable<TWorkItem> GetSubscribable<TWorkItem>();

		
	}
}
