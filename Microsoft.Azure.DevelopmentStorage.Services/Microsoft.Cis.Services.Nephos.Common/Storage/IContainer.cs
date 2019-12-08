using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IContainer : IDisposable
	{
		IStorageAccount Account
		{
			get;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Each property provides a direct 1-to-1 mapping with the XStore API, so seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ApplicationMetadata
		{
			get;
			set;
		}

		string ContainerName
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.Storage.ContainerType ContainerType
		{
			get;
		}

		DateTime? LastModificationTime
		{
			get;
		}

		ILeaseInfo LeaseInfo
		{
			get;
		}

		Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Common.ProviderInjection ProviderInjection
		{
			get;
			set;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Each property provides a direct 1-to-1 mapping with the XStore API, so seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ServiceMetadata
		{
			get;
			set;
		}

		TimeSpan Timeout
		{
			get;
			set;
		}

		IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, AsyncCallback callback, object state);

		IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, CacheRefreshOptions cacheRefreshOptions, bool shouldUpdateCacheEntryOnRefresh, AsyncCallback callback, object state);

		IAsyncResult BeginGetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state);

		IAsyncResult BeginSetProperties(ContainerPropertyNames propertyNames, IContainerCondition condition, AsyncCallback callback, object state);

		void EndGetProperties(IAsyncResult ar);

		void EndSetProperties(IAsyncResult ar);
	}
}