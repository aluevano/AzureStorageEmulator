using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class PerRequestStorageManager : IStorageManager, IDisposable
	{
		private IStorageManager sharedStorageManager;

		private Microsoft.Cis.Services.Nephos.Common.OperationStatus operationStatus;

		private Microsoft.Cis.Services.Nephos.Common.ProviderInjection providerInjection;

		private IStorageStamp internalStamp;

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.operationStatus;
			}
		}

		public Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get
			{
				return this.providerInjection;
			}
			set
			{
				this.providerInjection = value;
			}
		}

		private PerRequestStorageManager()
		{
		}

		public PerRequestStorageManager(IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.OperationStatus opStatus)
		{
			this.sharedStorageManager = storageManager;
			this.operationStatus = opStatus;
		}

		public IStorageAccount CreateAccountInstance(string accountName)
		{
			IStorageAccount operationStatus = this.sharedStorageManager.CreateAccountInstance(accountName);
			operationStatus.OperationStatus = this.OperationStatus;
			return operationStatus;
		}

		public IBlobContainer CreateBlobContainerInstance(string accountName, string containerName)
		{
			IBlobContainer operationStatus = this.sharedStorageManager.CreateBlobContainerInstance(accountName, containerName);
			operationStatus.OperationStatus = this.OperationStatus;
			operationStatus.ProviderInjection = this.ProviderInjection;
			return operationStatus;
		}

		public IQueueContainer CreateQueueContainerInstance(string accountName, string queueName)
		{
			IQueueContainer operationStatus = this.sharedStorageManager.CreateQueueContainerInstance(accountName, queueName);
			operationStatus.OperationStatus = this.OperationStatus;
			return operationStatus;
		}

		public IStorageStamp CreateStorageStampInstance()
		{
			if (this.internalStamp == null)
			{
				this.internalStamp = this.sharedStorageManager.CreateStorageStampInstance();
				this.internalStamp.OperationStatus = this.OperationStatus;
			}
			return this.internalStamp;
		}

		public ITableContainer CreateTableContainerInstance(string accountName, string tableName)
		{
			ITableContainer operationStatus = this.sharedStorageManager.CreateTableContainerInstance(accountName, tableName);
			operationStatus.OperationStatus = this.OperationStatus;
			return operationStatus;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && this.sharedStorageManager != null)
			{
				this.sharedStorageManager.Dispose();
				this.sharedStorageManager = null;
			}
		}

		public void Initialize()
		{
			this.sharedStorageManager.Initialize();
		}

		public void Shutdown()
		{
			this.sharedStorageManager.Shutdown();
		}
	}
}