using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobSummary : IDisposable
	{
		long? ExpiredBlobCount
		{
			get;
		}

		string Name
		{
			get;
		}

		long? TotalExpiredBlobContentLength
		{
			get;
		}

		long? TotalExpiredBlobMetadataSize
		{
			get;
		}

		long? TotalUnexpiredBlobContentLength
		{
			get;
		}

		long? TotalUnexpiredBlobMetadataSize
		{
			get;
		}

		long? UnexpiredBlobCount
		{
			get;
		}
	}
}