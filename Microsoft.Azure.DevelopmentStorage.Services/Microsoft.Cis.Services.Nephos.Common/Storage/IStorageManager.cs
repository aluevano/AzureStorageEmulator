using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IStorageManager : IDisposable
	{
		IStorageAccount CreateAccountInstance(string accountName);

		IBlobContainer CreateBlobContainerInstance(string accountName, string containerName);

		IQueueContainer CreateQueueContainerInstance(string accountName, string queueName);

		IStorageStamp CreateStorageStampInstance();

		ITableContainer CreateTableContainerInstance(string accountName, string tableName);

		void Initialize();

		void Shutdown();
	}
}