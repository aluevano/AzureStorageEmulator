using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IPutBlobResult
	{
		long? ContentCrc64
		{
			get;
			set;
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		byte[] ContentMD5
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