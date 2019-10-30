using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
	public interface IBufferManager<TWorkItem> : IBufferManager
	{
		Boolean TryGetNext(out TWorkItem item);
		IEnumerable<TWorkItem> GetAll();

		Boolean TryAddItem(TWorkItem item);		
	}

	public interface IBufferManager
	{
		Int32? AvailableBuffer { get; }
		Int32? MaximumBuffer { get; }
		Int32 BufferSize { get; }

		Int32 TotalProcessed { get; set; }

		Int32 TotalPublished { get; set; }

		Boolean HasAvailableTask { get; }
	}
}
