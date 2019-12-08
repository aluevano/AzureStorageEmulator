using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class ServiceRequestHandler
	{
		private static int concurrentRequestCount;

		public static int ConcurrentRequestCount
		{
			get
			{
				return ServiceRequestHandler.concurrentRequestCount;
			}
		}

		public abstract Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get;
		}

		public abstract IStorageManager StorageManagerProvider
		{
			get;
		}

		static ServiceRequestHandler()
		{
		}

		protected ServiceRequestHandler()
		{
		}

		public abstract void AcceptRequest(RequestContext requestContext);

		public abstract void AcceptRequest(RequestContext requestContext, Microsoft.Cis.Services.Nephos.Common.ServiceType serviceType);

		public static void DispatchRequest(IProcessor requestProcessor)
		{
			try
			{
				int num = Interlocked.Increment(ref ServiceRequestHandler.concurrentRequestCount);
				Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Entering concurrent request: {0}\n", new object[] { num });
				requestProcessor.OverallConcurrentRequestCount = num;
				requestProcessor.BeginProcess(new AsyncCallback(ServiceRequestHandler.ProcessAsyncCallback), requestProcessor);
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
				object[] str = new object[] { exception.ToString() };
				unhandledException.Log("BeginProcess threw exception {0}", str);
				int num1 = Interlocked.Decrement(ref ServiceRequestHandler.concurrentRequestCount);
				Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Leaving concurrent request: {0}\n", new object[] { num1 });
				requestProcessor.Dispose();
				EndToEndLogging.LogActivityEnd(Logger<IRestProtocolHeadLogger>.Instance.Verbose, "RDStorageRequest");
				throw;
			}
		}

		public static void DispatchRequestWorkItem(object requestProcessor)
		{
			ServiceRequestHandler.DispatchRequest((IProcessor)requestProcessor);
		}

		public virtual bool GetKeepAliveToFalseIfSlbProbeIsDownDCSettings()
		{
			return false;
		}

		private static void ProcessAsyncCallback(IAsyncResult ar)
		{
			try
			{
				Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("In the process end callback");
				using (IProcessor asyncState = (IProcessor)ar.AsyncState)
				{
					try
					{
						asyncState.EndProcess(ar);
					}
					catch (NephosAssertionException nephosAssertionException1)
					{
						NephosAssertionException nephosAssertionException = nephosAssertionException1;
						IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
						object[] str = new object[] { nephosAssertionException.ToString() };
						error.Log("ASSERTION: {0}", str);
						throw;
					}
					catch (Exception exception1)
					{
						Exception exception = exception1;
						IStringDataEventStream unhandledException = Logger<IRestProtocolHeadLogger>.Instance.UnhandledException;
						object[] objArray = new object[] { exception.ToString() };
						unhandledException.Log("EXCEPTION thrown: {0}", objArray);
					}
				}
			}
			finally
			{
				int num = Interlocked.Decrement(ref ServiceRequestHandler.concurrentRequestCount);
				Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Leaving concurrent request: {0}\n", new object[] { num });
				EndToEndLogging.LogActivityEnd(Logger<IRestProtocolHeadLogger>.Instance.Verbose, "RDStorageRequest");
			}
		}
	}
}