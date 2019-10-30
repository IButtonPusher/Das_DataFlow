using System;

namespace Das.DataFlow
{
	internal class CustomProgressReporter : IProgressive
	{
		private readonly IOverrideProgress _overrider;

		public CustomProgressReporter(IOverrideProgress overrider)
		{
			_overrider = overrider;
		}
		
		public Double PercentComplete => _overrider.PercentComplete;

		public override String ToString()
		{
			return "P: " + PercentComplete + "%";
		}
	}
}
