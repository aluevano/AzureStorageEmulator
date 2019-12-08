using System;
using System.Threading;

namespace AsyncHelper
{
	public static class AsyncHelpers
	{
		public static IAsyncResult BeginPerformOperation(OperationImpl methodImpl, string callerMethod, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>(callerMethod, callback, state);
			asyncIteratorContext.Begin(methodImpl(asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static IAsyncResult BeginSwitchToWorkerThread(AsyncCallback callback, object state)
		{
			AsyncResult<NoResults> asyncResult = new AsyncResult<NoResults>("AsyncHelperUtils.BeginSwitchToWorkerThread", callback, state);
			ThreadPool.QueueUserWorkItem(new WaitCallback(AsyncHelpers.WorkerThreadCallback), asyncResult);
			return asyncResult;
		}

		public static void EndPerformOperation(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public static void EndSwitchToWorkerThread(IAsyncResult ar)
		{
			Exception exception;
			((AsyncResult<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private static void WorkerThreadCallback(object state)
		{
			((AsyncResult<NoResults>)state).Complete(null, false);
		}
	}
}