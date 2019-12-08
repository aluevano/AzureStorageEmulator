using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class StorageManagerExtensions
	{
		public static IContainer CreateContainerInstance(this IStorageManager storageManager, string accountName, string containerName, ServiceType serviceType)
		{
			if (serviceType == ServiceType.BlobService)
			{
				return storageManager.CreateBlobContainerInstance(accountName, containerName);
			}
			NephosAssertionException.Fail("Invalid service type {0}.", new object[] { serviceType });
			return null;
		}
	}
}