using System;

namespace Das.DataFlow
{
	public class DefaultProgressReporter : IProgressive
	{
		private readonly IBufferManager _processor;

		public DefaultProgressReporter(IBufferManager processor)
		{
			_processor = processor;
		}

		//public event EventHandler Starting;
		//public event EventHandler<int> ProgressChanged;

		public Double PercentComplete
		{
			get
			{
				var denom = _processor.TotalProcessed +
				_processor.BufferSize;

				if (denom == 0)
					return 0;
				return _processor.TotalProcessed / (Double)denom;
			}
		}

		public override String ToString()
		{
			return "P: " + PercentComplete + "%";
		}


		public Boolean IsStarted => throw new NotImplementedException();
		public Boolean IsFinished => throw new NotImplementedException();
	}
}
