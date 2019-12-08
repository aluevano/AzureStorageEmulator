using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public sealed class DbStorageManager : IStorageManager, IDisposable
	{
		private const int MAXCONCURRENTSYNCHRONOUSCALLS = 5;

		private const int MAXPENDINGSYNCHRONOUSCALLS = 10000;

		internal Microsoft.Cis.Services.Nephos.Common.AsyncProcessor AsyncProcessor
		{
			get;
			private set;
		}

		public DbStorageManager()
		{
			this.AsyncProcessor = new Microsoft.Cis.Services.Nephos.Common.AsyncProcessor(5, 10000);
		}

		public long ComputeCrc(byte[] inputData, int offset, int count)
		{
			return (long)0;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		private void Dispose(bool disposing)
		{
			if (disposing && this.AsyncProcessor != null)
			{
				this.AsyncProcessor.Dispose();
				this.AsyncProcessor = null;
			}
		}

		IStorageAccount Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.CreateAccountInstance(string accountName)
		{
			return ((IStorageManager)this).CreateStorageStampInstance().CreateAccountInstance(accountName);
		}

		IBlobContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.CreateBlobContainerInstance(string accountName, string containerName)
		{
			return ((IStorageManager)this).CreateAccountInstance(accountName).CreateBlobContainerInstance(containerName);
		}

		IQueueContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.CreateQueueContainerInstance(string accountName, string queueName)
		{
			return ((IStorageManager)this).CreateAccountInstance(accountName).CreateQueueContainerInstance(queueName);
		}

		IStorageStamp Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.CreateStorageStampInstance()
		{
			IStorageStamp storageStamp;
			try
			{
				storageStamp = new StorageStamp(new DbStorageStamp(this));
			}
			catch (Exception exception)
			{
				SqlExceptionManager.ReThrowException(exception);
				throw;
			}
			return storageStamp;
		}

		ITableContainer Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.CreateTableContainerInstance(string accountName, string tableName)
		{
			return ((IStorageManager)this).CreateAccountInstance(accountName).CreateTableContainerInstance(tableName);
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.Initialize()
		{
			this.AsyncProcessor.Initialize();
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IStorageManager.Shutdown()
		{
			try
			{
				this.AsyncProcessor.Shutdown();
			}
			catch (Exception exception)
			{
			}
		}
	}
}