using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class StorageAccountExtensions
	{
		public static IAsyncResult BeginListContainers(this IStorageAccount account, ServiceType serviceType, string containerName, ContainerPropertyNames propertyNames, string separator, string containerNameStart, IContainerCondition condition, int maxContainerNames, AsyncCallback callback, object state)
		{
			if (serviceType == ServiceType.BlobService)
			{
				return account.BeginListBlobContainers(containerName, propertyNames, separator, containerNameStart, condition, maxContainerNames, callback, state);
			}
			NephosAssertionException.Fail("Invalid service type {0}.", new object[] { serviceType });
			return null;
		}

		public static IBlobContainerCollection EndListContainers(this IStorageAccount account, ServiceType serviceType, IAsyncResult ar)
		{
			if (serviceType == ServiceType.BlobService)
			{
				return account.EndListBlobContainers(ar);
			}
			NephosAssertionException.Fail("Invalid service type {0}.", new object[] { serviceType });
			return null;
		}
	}
}