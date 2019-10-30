using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    public interface ISeriesBuilder : IProgressive, IFinite, IDisposable
    {
		Boolean HasRelays { get; }

		IWorker GetSeriesWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer);

        IWorker GetSeriesWorker<TWorkItem>(IWorker<TWorkItem> worker);

		ISeriesProcessor GetSeriesWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker,
			Int32 maximumBuffer);

        ISeriesProcessor GetSeriesWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker);

		IEnumerable<Tuple<ISeriesProcessor, Int32?>> GetTaskedRelays();

	    void AddRelay(ISeriesProcessor series);

		IEnumerable<ISeriesProcessor> GetAllProcessors();

	}
}
