using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum BlobSummaryPropertyNames
	{
		None = 0,
		UnexpiredBlobCount = 1,
		TotalUnexpiredBlobContentLength = 2,
		TotalUnexpiredBlobMetadataSize = 4,
		ExpiredBlobCount = 256,
		TotalExpiredBlobContentLength = 512,
		TotalExpiredBlobMetadataSize = 1024
	}
}