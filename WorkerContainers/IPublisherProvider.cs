using System;

namespace Das.DataFlow
{
    internal interface IPublisherProvider : IProgressNotifier, IPublisherContainer,
        IFiniteNotifier, IControlDataFlow
	{
		Boolean IsPublishedDataAvailable { get; }

	    void AssignDataSources(ISeriesProcessor processor);

		Boolean TryAddSubscriber<TInput>(IDataSubscriber<TInput> subscriber);
	}
}
