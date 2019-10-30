using System;
using System.Collections;
using System.Collections.Generic;

namespace Das.DataFlow
{
    internal class PublisherEnumerator<TWorkProduct> : IDataSubscriber<TWorkProduct>,
		    IEnumerable<TWorkProduct>
    {
	    private readonly List<TWorkProduct> _results;

	    public PublisherEnumerator()
	    {
		    _results = new List<TWorkProduct>();
	    }

	    public void AddData(TWorkProduct record)
	    {
		    _results.Add(record);
	    }

	    public void AddData(TWorkProduct record, Int64 sizeCoefficient)
	    {
		    _results.Add(record);
	    }

	    public IEnumerator<TWorkProduct> GetEnumerator() => _results.GetEnumerator();

	    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	    public Type GetTypeSubscribedTo() => typeof(TWorkProduct);
    }
}
