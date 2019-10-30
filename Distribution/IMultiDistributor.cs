using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
	public interface IMultiDistributor<in TData> : IDistributor<TData>
    {
		Int32 Distribute(IEnumerable<TData> item, Int32 maxToPublish);

		Int32 Distribute(IEnumerable<TData> item);
	}
}
