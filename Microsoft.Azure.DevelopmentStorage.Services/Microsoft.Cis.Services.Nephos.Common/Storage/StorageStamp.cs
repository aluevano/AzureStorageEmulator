using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class StorageStamp : IStorageStamp, IDisposable
	{
		public static TranslateExceptionDelegate TranslateException;

		private bool disposed;

		private IStorageStamp internalStamp;

		public Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get
			{
				return this.internalStamp.OperationStatus;
			}
			set
			{
				this.internalStamp.OperationStatus = value;
			}
		}

		public TimeSpan Timeout
		{
			get
			{
				return this.internalStamp.Timeout;
			}
			set
			{
				try
				{
					this.internalStamp.Timeout = StorageStampHelpers.AdjustTimeoutRange(value);
				}
				catch (Exception exception)
				{
					StorageStamp.TranslateException(exception);
					throw;
				}
			}
		}

		public StorageStamp(IStorageStamp internalStamp)
		{
			this.internalStamp = internalStamp;
			this.disposed = false;
		}

		public IStorageAccount CreateAccountInstance(string accountName, ITableServerCommandFactory tableServerCommandFactory = null)
		{
			IStorageAccount storageAccount;
			try
			{
				IStorageAccount operationStatus = this.internalStamp.CreateAccountInstance(accountName, tableServerCommandFactory);
				StorageAccount storageAccount1 = new StorageAccount(operationStatus);
				operationStatus.OperationStatus = this.OperationStatus;
				storageAccount = storageAccount1;
			}
			catch (Exception exception)
			{
				StorageStamp.TranslateException(exception);
				throw;
			}
			return storageAccount;
		}

		public void Dispose()
		{
			this.Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing && !this.disposed)
			{
				this.internalStamp.Dispose();
				this.disposed = true;
			}
		}
	}
}