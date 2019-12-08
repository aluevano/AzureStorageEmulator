using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbStorageStamp : IStorageStamp, IDisposable
	{
		TimeSpan Microsoft.Cis.Services.Nephos.Common.Storage.IStorageStamp.Timeout
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		internal DbStorageManager StorageManager
		{
			get;
			private set;
		}

		internal DbStorageStamp(DbStorageManager storageManager)
		{
			this.StorageManager = storageManager;
		}

		IStorageAccount Microsoft.Cis.Services.Nephos.Common.Storage.IStorageStamp.CreateAccountInstance(string accountName, ITableServerCommandFactory tableServerCommandFactory)
		{
			return new DbStorageAccount(this, accountName);
		}

		void System.IDisposable.Dispose()
		{
		}
	}
}