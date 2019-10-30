using System;

namespace Das.DataFlow
{
    public interface IRelayProcessor<in TInput, out TOutput> : IDataSubscribable<TOutput>
	{
    }
}
