using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Das.DataFlow
{
    internal class CorePublisherContainer : IPublisherContainer
    {
	    public Int32 TotalRecordsPublishing => Publishers?.
			Sum(p => p.TotalRecordsPublishing) ?? 0;

		private readonly ITypeFinder _typeFinder;
	    private readonly List<IEnumerable> _collectionSearch;
		protected readonly HashSet<IDataPublisher> Publishers;

	    public CorePublisherContainer(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
			Publishers = new HashSet<IDataPublisher>();
			_collectionSearch = new List<IEnumerable> { Publishers };
		}

		public void AddPublisher<TWorkItem>(IDataPublisher<TWorkItem> publisher)
	    {
			Publishers.Add(publisher);
			PublisherAdded?.Invoke(this, publisher);
		}

	    public void AddPublisher(IDataPublisher publisher)
	    {
			Publishers.Add(publisher);
		    PublisherAdded?.Invoke(this, publisher);
		}

		public Int32 PublisherCount => Publishers.Count;

	    public IEnumerable<IDataPublisher> GetAllPublishers() => Publishers.ToArray();

		public IEnumerable<IDataSubscribable> GetAllDataSubscribables() => GetAllPublishers();
	    
	    public IDataPublisher<TOutput> GetPublisher<TOutput>()
	    {
			return Publishers.LastOrDefault(p => p is IDataPublisher<TOutput>)
				as IDataPublisher<TOutput>;
		}

	    public virtual IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
			=> GetDataSource<TWorkItem>(_collectionSearch);

	    public IDataSubscribable GetDataSource(IDataSubscriber forWorker)
	    {
			var argType = forWorker.GetType().GenericTypeArguments.FirstOrDefault();

			var imp = _typeFinder.GetImplementation<IDataSubscribable>(
				argType, GetAllPublishers());
			return imp;
		}

	    public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(
			IEnumerable<IEnumerable> toSearch)
	    {
			var tWork = typeof(TWorkItem);
		    var imp = _typeFinder.GetImplementation<IDataSubscribable>(tWork, toSearch);
		    return imp as IDataSubscribable<TWorkItem>;
		}

	    public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>(IEnumerable toSearch)
	    {
		    foreach (var some in toSearch)
		    {
				if (some is IDataSubscribable<TWorkItem> winner)
					return winner;
		    }

			return null;
	    }

		public IEnumerator<IDataPublisher> GetEnumerator() 
			=> GetAllPublishers().GetEnumerator();

	    public void Dispose()
	    {
		    
	    }

	    public event EventHandler<IDataPublisher> PublisherAdded;

	    IEnumerator IEnumerable.GetEnumerator()
	    {
		    return GetEnumerator();
	    }
    }
}
