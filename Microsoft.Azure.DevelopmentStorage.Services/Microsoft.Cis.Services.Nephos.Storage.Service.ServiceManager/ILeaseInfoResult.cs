using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ILeaseInfoResult
	{
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