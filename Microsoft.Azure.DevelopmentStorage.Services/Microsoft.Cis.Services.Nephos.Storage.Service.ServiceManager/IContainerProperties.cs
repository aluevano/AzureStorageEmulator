using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IContainerProperties
	{
		ContainerAclSettings ContainerAcl
		{
			get;
		}

		NameValueCollection ContainerMetadata
		{
			get;
		}

		long ContainerQuotaInGB
		{
			get;
		}

		DateTime LastModifiedTime
		{
			get;
		}

		ILeaseInfo LeaseInfo
		{
			get;
		}
	}
}