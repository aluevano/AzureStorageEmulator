using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobCopyInformation : IBlobGeoSenderInfo
	{
		byte[] CopyInfo
		{
			get;
		}
	}
}