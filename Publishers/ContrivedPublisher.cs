using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    internal class ContrivedPublisher<TWorkItem> : CorePublisher<TWorkItem>,
		IDataSubscriber<TWorkItem>
    {
	    public override event EventHandler<Double> ProgressChanged;
		public override Double PercentComplete => _completion;

		public override Int32 TotalRecordsPublishing => _totalRecordsPublishing;

		public override Boolean IsStarted { get; protected set; }

	    private readonly IControlDataFlow _startStopper;
	    private readonly IProgressNotifier _progress;
	    private readonly IDataSubscribable<TWorkItem> _dataSource;
		private Double _completion;
	    private Int32 _totalRecordsPublishing;


	    public ContrivedPublisher(IWorkSystem startStopper,
			IProgressNotifier progress,  IDataSubscribable<TWorkItem> dataSource)
		{
			_startStopper = startStopper;
			_progress = progress;
			_dataSource = dataSource;
			progress.ProgressChanged += OnProgressChanged;
			dataSource.AddSubscriber(this);
		}

	    private void OnProgressChanged(Object sender, Double e)
	    {
		    _completion = e;
			ProgressChanged?.Invoke(this, e);
	    }

		protected override void StartPublishing() => _startStopper.Start();

	    public override void Dispose()
	    {
		    if (_startStopper is IDisposable disposeMe)
				disposeMe.Dispose();
			if (_progress is IDisposable disposeMe2)
				disposeMe2.Dispose();
			if (_dataSource is IDisposable dontForgetMe)
				dontForgetMe.Dispose();
		}

	    public override IEnumerator<TWorkItem> GetEnumerator()
	    {
		    var forEnum = new PublisherEnumerator<TWorkItem>();
		    AddSubscriber(forEnum);
			Start();

			return forEnum.GetEnumerator();
	    }

	    public void AddData(TWorkItem record)
	    {
			_totalRecordsPublishing++;
			PublishData(record);
		}

		public void AddData(TWorkItem record, Int64 sizeCoefficient)
			=> AddData(record);
    }
}
