using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IListContainersResultContainerProperties
	{
		string ContainerName
		{
			get;
		}

		long ContainerQuotaInGB
		{
			get;
		}

		DateTime? LastModifiedTime
		{
			get;
		}

		string LeaseDuration
		{
			get;
		}

		string LeaseState
		{
			get;
		}

		string LeaseStatus
		{
			get;
		}

		NameValueCollection Metadata
		{
			get;
		}

		string PublicAccessLevel
		{
			get;
		}
	}
}