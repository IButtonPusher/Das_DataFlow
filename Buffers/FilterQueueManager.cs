using System;

namespace Das.DataFlow
{
	internal class FilterQueueManager<TWorkItem> : QueueBufferManager<TWorkItem>
	{
		private readonly IFilterInputData<TWorkItem> _filter;

		public FilterQueueManager(IFilterInputData<TWorkItem> filter, Int32? maximumBuffer) 
			: base(maximumBuffer)
		{
			_filter = filter;
		}

		public override Boolean TryAddItem(TWorkItem item)
		{
			if (!_filter.IsValid(item))
				return false;
			return base.TryAddItem(item);
		}
	}
}
