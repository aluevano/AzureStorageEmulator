using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IPutBlockResult : ICrc64Md5Result
	{
		bool IsWriteEncrypted
		{
			get;
		}
	}
}