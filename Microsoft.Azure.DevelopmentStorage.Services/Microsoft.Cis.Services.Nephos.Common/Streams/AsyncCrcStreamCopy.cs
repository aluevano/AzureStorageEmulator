using AsyncHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public static class AsyncCrcStreamCopy
	{
		private static IEnumerator<IAsyncResult> AsyncStreamCopyImpl(CrcStream source, CrcStream destination, long numBytes, int bufferSize, AsyncCrcStreamCopy.ComputeAndLogCrcMethod computeCrcMethodForCrcLogging, TimeSpan timeout, AsyncIteratorContext<long> context)
		{
			Exception exception;
			BufferWrapper buffer = null;
			BufferWrapper bufferWrapper = null;
			try
			{
				Duration startingNow = Duration.StartingNow;
				IAsyncResult asyncResult = null;
				IAsyncResult asyncResult1 = null;
				buffer = BufferPool.GetBuffer(bufferSize);
				bufferWrapper = BufferPool.GetBuffer(bufferSize);
				long num = (long)0;
				long? nullable = null;
				context.ResultData = (long)0;
				bool canTimeout = source.CanTimeout;
				bool flag = destination.CanTimeout;
				while (true)
				{
					int timeoutInMS = AsyncCrcStreamCopy.TimeSpanToTimeoutInMS(startingNow.Remaining(timeout));
					if (timeoutInMS == 0)
					{
						throw new TimeoutException("The asynchronous stream copy timed out.");
					}
					if (canTimeout)
					{
						try
						{
							source.ReadTimeout = timeoutInMS;
						}
						catch (InvalidOperationException invalidOperationException)
						{
							canTimeout = false;
						}
					}
					if (flag)
					{
						try
						{
							destination.WriteTimeout = timeoutInMS;
						}
						catch (InvalidOperationException invalidOperationException1)
						{
							flag = false;
						}
					}
					if (numBytes > (long)0)
					{
						asyncResult = source.BeginRead(buffer.Buffer, 0, (int)Math.Min((long)((int)buffer.Buffer.Length), numBytes), context.GetResumeCallback(), context.GetResumeState("AsyncCrcStreamCopy.source.Read"));
					}
					exception = null;
					if (num > (long)0)
					{
						try
						{
							asyncResult1 = destination.BeginWrite(bufferWrapper.Buffer, 0, (int)num, nullable, context.GetResumeCallback(), context.GetResumeState("AsyncCrcStreamCopy.dest.Write"));
						}
						catch (Exception exception1)
						{
							exception = exception1;
						}
						if (exception == null)
						{
							yield return asyncResult1;
							try
							{
								destination.EndWrite(asyncResult1);
							}
							catch (Exception exception2)
							{
								exception = exception2;
							}
						}
					}
					if (numBytes <= (long)0)
					{
						num = (long)0;
					}
					else
					{
						yield return asyncResult;
						num = (long)source.EndRead(out nullable, asyncResult);
					}
					if (exception != null)
					{
						throw ExceptionCloner.AttemptClone(exception, RethrowableWrapperBehavior.NoWrap);
					}
					if (num < (long)0)
					{
						break;
					}
					numBytes -= num;
					AsyncIteratorContext<long> resultData = context;
					resultData.ResultData = resultData.ResultData + num;
					if (computeCrcMethodForCrcLogging != null)
					{
						computeCrcMethodForCrcLogging(buffer.Buffer, 0, (int)num, context.ResultData);
					}
					AsyncCrcStreamCopy.Swap<BufferWrapper>(ref buffer, ref bufferWrapper);
					if (num <= (long)0)
					{
						goto Label2;
					}
				}
				throw new TimeoutException("Reading from the stream resulted in a negative number of bytes read. This typically means an HttpWebRequest was aborted.");
			}
			finally
			{
				if (buffer != null)
				{
					BufferPool.ReleaseBuffer(buffer);
				}
				if (bufferWrapper != null)
				{
					BufferPool.ReleaseBuffer(bufferWrapper);
				}
			}
		Label2:
			yield break;
			throw new TimeoutException("The asynchronous stream copy timed out.");
			throw ExceptionCloner.AttemptClone(exception, RethrowableWrapperBehavior.NoWrap);
		}

		public static IAsyncResult BeginAsyncStreamCopy(CrcStream source, CrcStream destination, int bufferSize, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return AsyncCrcStreamCopy.BeginAsyncStreamCopy(source, destination, bufferSize, null, timeout, callback, state);
		}

		public static IAsyncResult BeginAsyncStreamCopy(CrcStream source, CrcStream destination, int bufferSize, AsyncCrcStreamCopy.ComputeAndLogCrcMethod computeCrcMethodForCrcLogging, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<long> asyncIteratorContext = new AsyncIteratorContext<long>("AsyncCrcStreamCopy.AsyncCopyStream", callback, state);
			asyncIteratorContext.Begin(AsyncCrcStreamCopy.AsyncStreamCopyImpl(source, destination, 9223372036854775807L, bufferSize, computeCrcMethodForCrcLogging, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static IAsyncResult BeginAsyncStreamCopy(CrcStream source, CrcStream destination, long bytesToRead, int bufferSize, TimeSpan timeout, AsyncCallback callback, object state)
		{
			return AsyncCrcStreamCopy.BeginAsyncStreamCopy(source, destination, bytesToRead, bufferSize, null, timeout, callback, state);
		}

		public static IAsyncResult BeginAsyncStreamCopy(CrcStream source, CrcStream destination, long bytesToRead, int bufferSize, AsyncCrcStreamCopy.ComputeAndLogCrcMethod computeCrcMethodForCrcLogging, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<long> asyncIteratorContext = new AsyncIteratorContext<long>("AsyncCrcStreamCopy.AsyncCopyStream", callback, state);
			asyncIteratorContext.Begin(AsyncCrcStreamCopy.AsyncStreamCopyImpl(source, destination, bytesToRead, bufferSize, computeCrcMethodForCrcLogging, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public static long EndAsyncStreamCopy(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<long> asyncIteratorContext = (AsyncIteratorContext<long>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private static void Swap<T>(ref T a, ref T b)
		{
			T t = a;
			a = b;
			b = t;
		}

		public static int TimeSpanToTimeoutInMS(TimeSpan timeout)
		{
			if (timeout < TimeSpan.Zero)
			{
				throw new ArgumentOutOfRangeException("timeout", "Cannot be negative");
			}
			if (timeout == TimeSpan.MaxValue)
			{
				return -1;
			}
			double totalMilliseconds = timeout.TotalMilliseconds;
			if (totalMilliseconds >= 2147483647)
			{
				return 2147483647;
			}
			return (int)totalMilliseconds;
		}

		public delegate void ComputeAndLogCrcMethod(byte[] data, int offset, int count, long totalBytesReadSoFar);
	}
}