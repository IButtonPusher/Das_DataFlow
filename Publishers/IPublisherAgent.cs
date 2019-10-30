namespace Das.DataFlow
{
	public interface IPublisherAgent<in T>
    {
		void AddSubscriber<TWork>(IDataSubscriber<TWork> subscriber)
			where TWork : T;

		void PublishData(T data);
	}
}
