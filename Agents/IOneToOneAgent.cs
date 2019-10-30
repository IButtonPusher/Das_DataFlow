namespace Das.DataFlow
{
	public interface IOneToOneAgent<in TInput, in TOutput> : 
		IRelayAgent<TInput, TOutput>, IDistributor<TOutput>
	{
    }
}
