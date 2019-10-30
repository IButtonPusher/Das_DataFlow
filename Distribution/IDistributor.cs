namespace Das.DataFlow
{
	public interface IDistributor<in TData>
	{
		void Distribute(TData item);
	}
}
