namespace Das.DataFlow
{
	public interface IOneToManyAgent<in TInput, TOutput> :
		IRelayAgent<TInput, TOutput>, IMultiDistributor<TOutput>
	{
    }
}
