using System;
using System.Collections.Generic;

namespace Das.DataFlow
{
    public class VolatileFuncInvoker<TOutput, TInput> : VolatileActionInvoker,
        IActionInvoker<TOutput, TInput>
    {
        public VolatileFuncInvoker(Int32 retryCount, Int32 waitOnFail,
            Action<String> failLog)
            : base(retryCount, waitOnFail, failLog)
        {
            TotalTries = retryCount;
            FailLog = failLog;
        }

        public TOutput Invoke(Func<TInput, TOutput> func, TInput input)
        {
            for (var i = 0; i < TotalTries; i++)
            {
                try
                {
                    return func(input);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }

            return default;
        }

        public IEnumerable<TOutput> Invoke(Func<TInput, IEnumerable<TOutput>> func,
            TInput input)
        {
            for (var i = 0; i < TotalTries; i++)
            {
                try
                {
                    return func(input);
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }

            return null;
        }

        public TOutput Invoke(Func<TOutput> func)
        {
            for (var i = 0; i < TotalTries; i++)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }

            return default;
        }

        public IEnumerable<TOutput> Invoke(Func<IEnumerable<TOutput>> func)
        {
            for (var i = 0; i < TotalTries; i++)
            {
                try
                {
                    return func();
                }
                catch (Exception ex)
                {
                    HandleError(ex);
                }
            }

            return null;
        }
    }
}

