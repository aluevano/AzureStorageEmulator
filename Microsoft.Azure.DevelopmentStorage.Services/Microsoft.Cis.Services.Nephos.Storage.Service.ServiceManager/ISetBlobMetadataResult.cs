using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ISetBlobMetadataResult
	{
		bool IsWriteEncrypted
		{
			get;
		}

		DateTime LastModifiedTime
		{
			get;
		}
	}
}