using System;
using System.Collections;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal interface IInternalPublisherSelector : IPublisherSelector,
		ISubscribableDetector
	{
	    Int32 PublisherCount { get; }

		IEnumerable<IDataPublisher> GetAllPublishers();

		IEnumerable<IDataSubscribable> GetAllDataSubscribables();

		IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(IEnumerable<IEnumerable> toSearch);

		IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(IEnumerable toSearch);
	}
}
