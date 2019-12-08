using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IStorageAccount : IDisposable
	{
		string ClusterName
		{
			get;
			set;
		}

		bool IsSecondaryAccess
		{
			get;
			set;
		}

		DateTime? LastModificationTime
		{
			get;
			set;
		}

		string Name
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		AccountPermissions? Permissions
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get;
			set;
		}

		SecretKeyListV3 SecretKeysV3
		{
			get;
			set;
		}

		AccountServiceMetadata ServiceMetadata
		{
			get;
			set;
		}

		TimeSpan Timeout
		{
			get;
			set;
		}

		IAsyncResult BeginCreateQueueContainer(string queueName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state);

		IAsyncResult BeginCreateTableContainer(string tableName, DateTime? expiryTime, byte[] serviceMetadata, byte[] applicationMetadata, AsyncCallback callback, object state);

		IAsyncResult BeginDeleteQueueContainer(string queueName, IContainerCondition conditions, AsyncCallback callback, object state);

		IAsyncResult BeginDeleteTableContainer(string tableName, IContainerCondition conditions, AsyncCallback callback, object state);

		IAsyncResult BeginGetProperties(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncCallback callback, object state);

		IAsyncResult BeginListBlobContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state);

		IAsyncResult BeginListQueueContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state);

		IAsyncResult BeginListTableContainers(string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state);

		IAsyncResult BeginSetProperties(AccountPropertyNames propertyNames, IAccountCondition conditions, AsyncCallback callback, object state);

		IBlobContainer CreateBlobContainerInstance(string containerName);

		IQueueContainer CreateQueueContainerInstance(string queueName);

		ITableContainer CreateTableContainerInstance(string tableName);

		IQueueContainer EndCreateQueueContainer(IAsyncResult ar);

		ITableContainer EndCreateTableContainer(IAsyncResult ar);

		void EndDeleteQueueContainer(IAsyncResult ar);

		void EndDeleteTableContainer(IAsyncResult ar);

		void EndGetProperties(IAsyncResult ar);

		IBlobContainerCollection EndListBlobContainers(IAsyncResult ar);

		IQueueContainerCollection EndListQueueContainers(IAsyncResult ar);

		ITableContainerCollection EndListTableContainers(IAsyncResult ar);

		void EndSetProperties(IAsyncResult ar);
	}
}