using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Das.DataFlow
{
	internal class QueueBufferManager<TWorkItem> : IBufferManager<TWorkItem>
	{
		private readonly ConcurrentQueue<TWorkItem> _pendingItems;
		public Int32? MaximumBuffer { get; }

		public QueueBufferManager(Int32? maximumBuffer)
		{
			_pendingItems = new ConcurrentQueue<TWorkItem>();
			MaximumBuffer = maximumBuffer;
		}

		public Int32? AvailableBuffer => MaximumBuffer == null ? null
			: MaximumBuffer - _pendingItems.Count;

		public virtual Boolean HasAvailableTask => !_pendingItems.IsEmpty;

		public Int32 BufferSize => _pendingItems.Count;

		public Int32 TotalProcessed { get; set; }
		public Int32 TotalPublished { get; set; }

		public Double GetCompletionPercentage(Int32 totalProcessed)
			=> totalProcessed / (Double)(totalProcessed + _pendingItems.Count);

		public IEnumerable<TWorkItem> GetAll()
		{
			var all = new List<TWorkItem>();
			while (_pendingItems.TryDequeue(out var item))
				all.Add(item);

			return all;
		}

		public virtual Boolean TryAddItem(TWorkItem item)
		{
			_pendingItems.Enqueue(item);
			return true;
		}

		public override String ToString()
		{
			return "QBuffer " + "Qd: " + BufferSize + 
				(MaximumBuffer != null ? " (max: " + MaximumBuffer + ") " : "") +
				" procd: " + TotalProcessed + " pubd: " + TotalPublished;
		}

		public Boolean TryGetNext(out TWorkItem item)
			=> _pendingItems.TryDequeue(out item);
	}
}
