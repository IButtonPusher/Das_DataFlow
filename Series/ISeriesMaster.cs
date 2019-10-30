using System;

namespace Das.DataFlow
{
    internal interface ISeriesMaster : IFinite, IProgressNotifier
	{
		Boolean TryProcessNext();
    }
}
