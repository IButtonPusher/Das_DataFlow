using DataFlow.Interfaces;

namespace Das.DataFlow
{
	public interface IRelayAgent<in TInput, in TOutput> : IPublisherAgent<TOutput>,
		IWorkManager<TInput>
	{
    }
}
