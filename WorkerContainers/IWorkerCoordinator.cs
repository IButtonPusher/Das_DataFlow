using System;

namespace Das.DataFlow
{
    internal interface IWorkerCoordinator : IWorkerContainer, IPublisherProvider
	{
	    Boolean IsReadyToStart { get; }
	}
}
