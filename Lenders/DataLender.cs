using System;
using Das.DataCore;

namespace Das.DataFlow
{
    public class DataLender<T> : LenderCore<T> where T : ILendable<T>, new()
	{
		protected override T GetInternal() 
			=> new T { ReturnToSender = Put };
	}
}
