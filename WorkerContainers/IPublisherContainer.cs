using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    internal interface IPublisherContainer : IPublisherAdder, IInternalPublisherSelector, 
		IDisposable, IEnumerable<IDataPublisher>
	{
		event EventHandler<IDataPublisher> PublisherAdded;

		Int32 TotalRecordsPublishing { get; }
	}
}
