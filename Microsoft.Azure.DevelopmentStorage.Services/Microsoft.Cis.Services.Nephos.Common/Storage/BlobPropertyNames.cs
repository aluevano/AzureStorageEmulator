using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum BlobPropertyNames
	{
		None = 0,
		ContentType = 1,
		ContentLength = 2,
		CreationTime = 4,
		LastModificationTime = 8,
		ServiceMetadata = 32,
		ApplicationMetadata = 64,
		BlobType = 128,
		LeaseType = 256,
		SequenceNumber = 512,
		Flags = 1024,
		AnchorSize = 2048,
		TotalBlobSize = 4096,
		TotalMetadataSize = 8192,
		Size = 16384,
		DeltaSize = 32768,
		CommittedBlockCount = 65536,
		RawApplicationMetadata = 131072
	}
}