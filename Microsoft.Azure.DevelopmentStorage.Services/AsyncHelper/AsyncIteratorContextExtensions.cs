using System;
using System.Runtime.CompilerServices;

namespace AsyncHelper
{
	public static class AsyncIteratorContextExtensions
	{
		private static void End(this IAsyncResult asyncResult, RethrowableWrapperBehavior wrapperBehavior = 0)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext asyncIteratorContext = asyncResult as AsyncIteratorContext;
			if (asyncIteratorContext == null)
			{
				throw new InvalidOperationException(string.Format("End may only be called on result of type '{0}', given type is '{1}'", typeof(AsyncIteratorContext), asyncResult.GetType()));
			}
			asyncIteratorContext.End(out exception, wrapperBehavior);
			if (exception != null)
			{
				throw exception;
			}
		}

		public static TResultData End<TResultData>(this IAsyncResult asyncResult, RethrowableWrapperBehavior wrapperBehavior = 0)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<TResultData> asyncIteratorContext = (AsyncIteratorContext<TResultData>)asyncResult;
			if (asyncIteratorContext == null)
			{
				throw new InvalidOperationException(string.Format("End may only be called on result of type '{0}', given type is '{1}'", typeof(AsyncIteratorContext<TResultData>), asyncResult.GetType()));
			}
			asyncIteratorContext.End(out exception, wrapperBehavior);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}
	}
}