using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DataFlow.Interfaces;

namespace Das.DataFlow
{
	/// <summary>
	/// returns ISeriesProcessors and IWorkers that read from and write to queues, respectively
	/// </summary>
    internal class QueuedSeriesBuilder : ISeriesBuilder
    {
	    protected readonly List<ISeriesProcessor> RelayProcesses;
		private readonly IPublisherContainer _publishers;
	    //private readonly IWorkerContainer _workers;

	    public QueuedSeriesBuilder(IPublisherContainer publishers)
	    {
		    _publishers = publishers;
		    RelayProcesses = new List<ISeriesProcessor>();
		}

       

        public ISeriesProcessor GetSeriesWorker<TInput, TOutput>(
			IRelayWorker<TInput, TOutput> worker, Int32 maximumBuffer)
        {
            return GetSeriesWorker(worker,
                () => GetBufferManager<TInput, TOutput>(worker, maximumBuffer));
	    }

        public ISeriesProcessor GetSeriesWorker<TInput, TOutput>(IRelayWorker<TInput, TOutput> worker)
        {
            return GetSeriesWorker(worker,
                () => GetBufferManager<TInput, TOutput>(worker));
        }

        private ISeriesProcessor GetSeriesWorker<TInput, TOutput>(
            IRelayWorker<TInput, TOutput> worker, Func<IBufferManager<TInput>> getBuffer)
        {
            IRelayAgent<TInput, TOutput> distrib;
            switch (worker)
            {
                case IOneToOneWorker<TInput, TOutput> oneToOne:
                    distrib = new OneToOneAgent<TInput, TOutput>(oneToOne);
                    break;
                case IOneToManyWorker<TInput, TOutput> oneToMany:
                    distrib = new OneToManyAgent<TInput, TOutput>(oneToMany);
                    break;
                default:
                    throw new NotImplementedException();
            }

            var buffer = getBuffer();
            var progress = GetProgressReporter(worker, buffer);

            var chainingTo = GetDataSource<TInput>();
            var newSeries = new RelayQueueProcessor<TInput, TOutput>(distrib, buffer, progress,
                chainingTo);
		   
            return newSeries;
        }

       

        private IDataSubscribable<TInput> GetDataSource<TInput>()
			=> _publishers.GetDataSource<TInput>(RelayProcesses)
			    ?? _publishers.GetDataSource<TInput>();
		

		public IEnumerable<Tuple<ISeriesProcessor, Int32?>> GetTaskedRelays()
		{
			for (Int32 i = 0; i < RelayProcesses.Count - 1; i++)
			{
				var proc = RelayProcesses[i];

				if (!proc.HasAvailableTask)
					continue;

				var pushCap = RelayProcesses[i + 1].AvailableBuffer;

				if (pushCap == 0)
					continue;

				yield return new Tuple<ISeriesProcessor, Int32?>(proc, pushCap);
			}

			yield return new Tuple<ISeriesProcessor, Int32?>(
				RelayProcesses[RelayProcesses.Count - 1], null);
		}

	    public void AddRelay(ISeriesProcessor series)
	    {
			RelayProcesses.Add(series);
		}

		public IEnumerable<ISeriesProcessor> GetAllProcessors()
			=> RelayProcesses.ToArray();

	    public Boolean IsStarted => _publishers.Any(p => p.IsStarted);

	    public Boolean IsFinished => RelayProcesses.Any() &&
			RelayProcesses.All(p => !p.IsActivelyWorking && !p.HasAvailableTask);

	    public Boolean IsCancelled => throw new NotSupportedException();

		public Boolean HasRelays => RelayProcesses.Any();
        
        public IWorker GetSeriesWorker<TWorkItem>(IWorker<TWorkItem> worker)
        {
            IBufferManager<TWorkItem> GetBuffer() 
                => GetBufferManager<TWorkItem, Object>(worker);

            return GetSeriesWorker(worker, GetBuffer);
        }


		public IWorker GetSeriesWorker<TWorkItem>(IWorker<TWorkItem> worker, Int32 maximumBuffer)
        {
            IBufferManager<TWorkItem> GetBuffer() 
                => GetBufferManager<TWorkItem, Object>(worker, maximumBuffer);

            return GetSeriesWorker(worker, GetBuffer);
        }

        private IWorker GetSeriesWorker<TWorkItem>(IWorker<TWorkItem> worker, 
            Func<IBufferManager<TWorkItem>> getBufferManager)
        {
            switch (worker)
            {
                case ISeriesProcessor<TWorkItem> series:
                    return series;
                case IDataPublisher<TWorkItem> publisher:
                    return publisher;
                case IWorkManager<TWorkItem> manager:
                    return Build(manager, worker, getBufferManager);
                case IOneToNoneWorker<TWorkItem> ender:
                    var distrib = new OneToNoneManager<TWorkItem>(ender);
                    return Build(distrib, worker, getBufferManager);
                default:
                    throw new NotImplementedException();
            }
        }

		private IWorker Build<TWorkItem>(IWorkManager<TWorkItem> distrib,
			IWorker<TWorkItem> worker, Func<IBufferManager<TWorkItem>> getBufferManager)
        {
            var buffer = getBufferManager();
			var progress = GetProgressReporter(worker, buffer);
			var chainingTo = GetDataSource<TWorkItem>();

			var newSeries = new SeriesProcessor<TWorkItem>(distrib,
				progress, buffer, chainingTo);
			
			return newSeries;
		}

	    private IBufferManager<TInput> GetBufferManager<TInput, TOutput>(IWorker<TInput> worker,
		    Int32 maximumBuffer)
	    {
		    switch (worker)
		    {
				case IBufferManager<TInput> buffer:
					return buffer;
			    case IFilterInputData<TInput> filters:
				    return new FilterQueueManager<TInput>(filters, maximumBuffer);
			    case IRelayWorker<TInput, TOutput> _:
			    case IOneToNoneWorker<TInput> _:
				case IWorkManager<TInput> _:
				    return new QueueBufferManager<TInput>(maximumBuffer);
			    default:
				    throw new NotImplementedException();
		    }
	    }

        private IBufferManager<TInput> GetBufferManager<TInput, TOutput>(IWorker<TInput> worker)
        {
            switch (worker)
            {
                case IBufferManager<TInput> buffer:
                    return buffer;
                case IFilterInputData<TInput> filters:
                    return new FilterQueueManager<TInput>(filters, null);
                case IRelayWorker<TInput, TOutput> _:
                case IOneToNoneWorker<TInput> _:
                case IWorkManager<TInput> _:
                    return new QueueBufferManager<TInput>(null);
                default:
                    throw new NotImplementedException();
            }
        }

	    private IProgressive GetProgressReporter(IWorker worker, IBufferManager buffer)
	    {
		    switch (worker)
		    {
			    case IOverrideProgress overrider:
				    return new CustomProgressReporter(overrider);
			    default:
				    return new DefaultProgressReporter(buffer);
		    }
	    }

		public override String ToString()
		{
			var sb = new StringBuilder();
			foreach (var p in RelayProcesses)
				sb.AppendLine(p.ToString());
			return sb.ToString();
		}

	    public void Dispose()
	    {
		    for (var i = 0; i < RelayProcesses.Count; i++)
			    RelayProcesses[i].Dispose();

			_publishers.Dispose();

		}

		Double IProgressive.PercentComplete => 1;
    }
}
