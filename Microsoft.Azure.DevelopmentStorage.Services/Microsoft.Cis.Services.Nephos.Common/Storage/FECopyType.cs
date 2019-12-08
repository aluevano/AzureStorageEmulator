using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum FECopyType
	{
		Default = -2,
		Sync = -1,
		Async = 0,
		Link = 1,
		CoR = 2,
		SyncLink = 3,
		Incremental = 4
	}
}