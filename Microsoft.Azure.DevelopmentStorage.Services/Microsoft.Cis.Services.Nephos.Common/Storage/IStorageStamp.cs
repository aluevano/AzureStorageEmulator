using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IStorageStamp : IDisposable
	{
		Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		TimeSpan Timeout
		{
			get;
			set;
		}

		IStorageAccount CreateAccountInstance(string accountName, ITableServerCommandFactory tableServerCommandFactory);
	}
}