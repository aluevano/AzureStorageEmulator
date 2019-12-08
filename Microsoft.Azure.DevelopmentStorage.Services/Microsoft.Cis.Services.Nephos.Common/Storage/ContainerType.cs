using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum ContainerType
	{
		None = 0,
		BlobContainer = 1,
		TableContainer = 2,
		QueueContainer = 3,
		FileContainer = 8,
		XioTableContainer = 9
	}
}