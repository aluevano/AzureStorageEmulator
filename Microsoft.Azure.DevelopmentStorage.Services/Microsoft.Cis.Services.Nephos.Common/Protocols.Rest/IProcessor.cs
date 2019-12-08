using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface IProcessor : IDisposable
	{
		int OverallConcurrentRequestCount
		{
			set;
		}

		Microsoft.Cis.Services.Nephos.Common.ServiceType ServiceType
		{
			get;
		}

		IAsyncResult BeginProcess(AsyncCallback callback, object state);

		void EndProcess(IAsyncResult result);

		void IndicateComplete(bool success);

		event EventHandler<ProcessorCompletionEventArgs> ProcessorCompletion;

		event EventHandler ProcessorStarted;
	}
}