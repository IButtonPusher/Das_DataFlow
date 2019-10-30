using System;


namespace Das.DataFlow
{
    internal class ContrivedOneToOneSubscribable<TInput, TOutput>
		: CoreSubscribable<TOutput>,
		IOneToOneWorker<TInput, TOutput>
    {
	    private readonly IOneToOneWorker<TInput, TOutput> _enveloping;

	    public ContrivedOneToOneSubscribable(
			IOneToOneWorker<TInput, TOutput> enveloping)
	    {
		    _enveloping = enveloping;
	    }

	    public TOutput Process(TInput input)
	    {
			var res = _enveloping.Process(input);
			SendToSubscribers(res);

			return res;
		}
    }
}
