using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface ITableServerCommand<out TResult> : ITableServerCommand, IDisposable
	{
		TResult EndExecute(IAsyncResult asyncResult);
	}
}