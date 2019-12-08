using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface ITableServerCommand : IDisposable
	{
		TimeSpan Timeout
		{
			get;
			set;
		}

		IAsyncResult BeginExecute(AsyncCallback asyncCallback, object state);

		void EndExecute(IAsyncResult asyncResult);
	}
}