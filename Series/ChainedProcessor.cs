using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
// ReSharper disable ConvertToLocalFunction

namespace Das.DataFlow.Series
{
    internal class ChainedProcessor<TInput> : IDirectSeriesProcessor<TInput>,
		IDataSubscriber<TInput>
	{
		private readonly ITypeFinder _typeFinder;
		private readonly HashSet<Delegate> _openFuncs;
	    private readonly HashSet<Delegate> _connectedFuncs;
	    private Action<TInput> _lastFunction;

		public ChainedProcessor(ITypeFinder typeFinder)
		{
			_typeFinder = typeFinder;
			_openFuncs = new HashSet<Delegate>();
			_connectedFuncs = new HashSet<Delegate>();

			Func<TInput, TInput> seed = Run;
			_openFuncs.Add(seed);
		}

		private IEnumerable<Delegate> GetFuncs()
		{
			foreach (var d in _openFuncs.ToArray())
				yield return d;
			foreach (var d in _connectedFuncs.ToArray())
				yield return d;
		}

		public void Process(TInput workItem)
		{
			_lastFunction(workItem);
		}

		public TInput Run(TInput input)
		{
			return input;
		}

		public void AddRelayToChain<WorkerInput, TOutput>(Func<WorkerInput, TOutput> func)
		{
            var availableSources = GetFuncs();

			foreach (var d in availableSources)
			{
                if (!TryConnect(d, func)) 
                    continue;

                SetConnected(d);
                return;
            }

			throw new InvalidDataContractException();
		}

		public void AddIteratorToChain<WorkerInput, TOutput>(
			Func<WorkerInput, IEnumerable<TOutput>> func)
        {
            var availableSources = GetFuncs();

			foreach (var o in availableSources)
			{
                if (!TryConnectToIterator(o, func)) 
                    continue;

                SetConnected(o);
                return;
            }

			throw new InvalidDataContractException();
		}

		private void SetConnected(Delegate del)
		{
			_connectedFuncs.Add(del);
			_openFuncs.Remove(del);
		}

		private Boolean TryConnect<TWorkerInput, TOutput>(Delegate open,
			Func<TWorkerInput, TOutput> func)
		{
			switch (open)
			{
				case Func<TInput, TWorkerInput> oneToOne:
					ConnectOneToOne(oneToOne, func);
					return true;
				case Func<TInput, IEnumerable<TWorkerInput>> oneToMany:
					ConnectIterator(oneToMany, func);
					return true;
				default:
					return false;
			}
		}

		private Boolean TryConnectToIterator<WorkerInput, TOutput>(Delegate open,
			Func<WorkerInput, IEnumerable<TOutput>> func)
		{
			switch (open)
			{
				case Func<TInput, WorkerInput> oneToOne:
					ConnectOneToOne(oneToOne, func);
					return true;
				case Func<TInput, IEnumerable<WorkerInput>> oneToMany:
					IteratorToIterator(oneToMany, func);
					return true;
				default:
					return false;
			}
		}

		private void ConnectOneToOne<TWorkerInput, TOutput>(Func<TInput, TWorkerInput> input,
			Func<TWorkerInput, TOutput> output)
		{
			Func<TInput, TOutput> newComposed = f => output(input(f));
			_openFuncs.Add(newComposed);
            SetLastFunc(newComposed);
		}

		private void ConnectIterator<TWorkerInput, TOutput>(
			Func<TInput, IEnumerable<TWorkerInput>> input,
			Func<TWorkerInput, TOutput> output)
		{
			Func<TInput, IEnumerable<TOutput>> composedIterator = (f) =>
				BuildIterator(input, output, f);
			_openFuncs.Add(composedIterator);
            SetLastFunc(composedIterator);
		}

		private void IteratorToIterator<TWorkerInput, TOutput>(
			Func<TInput, IEnumerable<TWorkerInput>> input,
			Func<TWorkerInput, IEnumerable<TOutput>> output)
		{
			_connectedFuncs.Add(input);
			Func<TInput, IEnumerable<TOutput>> newComposed = f => 
				ComposeIterator(input, output, f);
			_openFuncs.Add(newComposed);

            SetLastFunc(newComposed);
		}

		/// <summary>
		/// F1(I)->O[] + F2(O)->R[] = F3(O[])->R[]
		/// No double enumerable
		/// </summary>
		private static IEnumerable<TRes> ComposeIterator<TReq, TRes>(
			Func<TInput, IEnumerable<TReq>> input,
		Func<TReq, IEnumerable<TRes>> output, TInput f)
		{
			foreach (var v in input(f))
				foreach (var r in output(v))
					yield return r;
		}

        private void SetLastFunc<TOutput>(Func<TInput, TOutput> func)
        {
            var action = ToAction(func);
            SetLastFunc(action);
        }

        private void SetLastFunc(Action<TInput> action)
        {
            _lastFunction = action;
        }


		/// <summary>
		/// F1(I)->O[] + F2(O)->R = F3(O[])->R[]
		/// </summary>
		private static IEnumerable<TRes> BuildIterator<TReq, TRes>(
            Func<TInput, IEnumerable<TReq>> input,
			Func<TReq, TRes> output, TInput f)
		{
			foreach (var v in input(f))
				yield return output(v);
		}


        public void AddToChainTerminator<TWorkerInput>(Action<TWorkerInput> meth)
		{
			foreach (var o in GetFuncs())
			{
				switch (o)
				{
					case Func<TInput, TWorkerInput> oneToOne:
						_connectedFuncs.Add(o);
						Action<TInput> newComposed = f => meth(oneToOne(f));
						_connectedFuncs.Add(newComposed);
                        SetLastFunc(newComposed);
                        
						return;
					case Func<TInput, IEnumerable<TWorkerInput>> oneToMany:
						Action<TInput> newComposed2 = (f) =>
						{
							foreach (var v in oneToMany(f))
								meth(v);
						};

						SetConnected(o);
						_connectedFuncs.Add(newComposed2);
                        SetLastFunc(newComposed2);

						return;
				}
			}

			throw new InvalidDataContractException();
		}

		public void AddToCappedChainTerminator<TWorkerInput>(
			ISeriesProcessor<TWorkerInput> workSystem, Action<TWorkerInput> addData)
		{
			foreach (var o in GetFuncs())
			{
				switch (o)
				{
					case Func<TInput, TWorkerInput> oneToOne:
						_connectedFuncs.Add(o);
						Action<TInput> newComposed = f =>
						{
							addData(oneToOne(f));
							if (workSystem.AvailableBuffer == 0)
								workSystem.TryProcess(null);
						};
						_connectedFuncs.Add(newComposed);
                        SetLastFunc(newComposed);
						
						return;
					case Func<TInput, IEnumerable<TWorkerInput>> oneToMany:
						Action<TInput> newComposed2 = (f) =>
						{
							foreach (var v in oneToMany(f))
							{
								addData(v);
								if (workSystem.AvailableBuffer == 0)
									workSystem.TryProcess(null);
							}
						};

						_connectedFuncs.Add(o);
						_connectedFuncs.Add(newComposed2);
                        SetLastFunc(newComposed2);

						return;
				}
			}

			throw new InvalidDataContractException();
		}



		private static Action<T1> ToAction<T1, T2>(Func<T1, T2> func)
		{
			return x => func(x);
		}

		public IDataSubscribable<TWorkItem> GetDataSource<TWorkItem>()
		{
			var tWork = typeof(TWorkItem);

			var imp = _typeFinder.GetImplementation<IDataSubscribable>(tWork,
				_openFuncs.OfType<IEnumerable>());
                
            if (imp == null)
				imp = _typeFinder.GetImplementation<IDataSubscribable>(tWork,
					_connectedFuncs.OfType<IEnumerable>());

			return imp as IDataSubscribable<TWorkItem>;
		}
		

		public void AddData(TInput record)
		{
			throw new NotImplementedException();
		}

		public void Dispose()
		{
			_openFuncs.Clear();
			_connectedFuncs.Clear();
			_lastFunction = null;
		}
	}
}
