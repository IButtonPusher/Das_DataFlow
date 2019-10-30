using System;

namespace Das.DataFlow
{
    internal interface IWorkerContainer : IWorkerAdder, IWorkerSelector, IDisposable,
	    ISubscribableDetector
	{	
	}
}
