using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IPutPageResult : ICrc64Md5Result
	{
		bool IsWriteEncrypted
		{
			get;
		}

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