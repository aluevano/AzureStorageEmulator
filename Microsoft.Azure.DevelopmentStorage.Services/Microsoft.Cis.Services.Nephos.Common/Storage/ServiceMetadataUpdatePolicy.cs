using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum ServiceMetadataUpdatePolicy
	{
		None,
		MergeAll,
		KeepList,
		RemoveList,
		AddIfNotExist
	}
}