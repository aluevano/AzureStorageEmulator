using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IClearPageResult
	{
		DateTime LastModifiedTime
		{
			get;
		}

		long SequenceNumber
		{
			get;
		}
	}
}