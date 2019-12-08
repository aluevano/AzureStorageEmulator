using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IAppendBlockResult : ICrc64Md5Result
	{
		long AppendOffset
		{
			get;
		}

		int CommittedBlockCount
		{
			get;
		}

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