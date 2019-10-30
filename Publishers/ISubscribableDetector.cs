using System;

namespace Das.DataFlow
{
    internal interface ISubscribableDetector
    {
	    /// <summary>
	    /// Picks the best data source for the worker parameter
	    /// </summary>
	    /// <typeparam name="TWorkItem"></typeparam>
	    /// <returns></returns>
	    /// <exception cref="InvalidOperationException">If no input source is found</exception>
	    IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>();

	    IDataSubscribable GetDataSource(IDataSubscriber forWorker);
	}
}
