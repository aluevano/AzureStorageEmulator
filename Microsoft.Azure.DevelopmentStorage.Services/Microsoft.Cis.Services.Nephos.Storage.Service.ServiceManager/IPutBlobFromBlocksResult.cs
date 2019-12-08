using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IPutBlobFromBlocksResult
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