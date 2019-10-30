using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    internal interface IDirectSeriesProcessor<in TWorkItem> : IDisposable
	{
		void Process(TWorkItem workItem);

		void AddToCappedChainTerminator<TWorkerInput>(
			ISeriesProcessor<TWorkerInput> workSystem, Action<TWorkerInput> addData);

		void AddToChainTerminator<TWorkerInput>(Action<TWorkerInput> meth);

		void AddRelayToChain<WorkerInput, TOutput>(Func<WorkerInput, TOutput> func);

		void AddIteratorToChain<WorkerInput, TOutput>(
			Func<WorkerInput, IEnumerable<TOutput>> func);

        IDataSubscribable<TData> GetDataSource<TData>();
    }
}
