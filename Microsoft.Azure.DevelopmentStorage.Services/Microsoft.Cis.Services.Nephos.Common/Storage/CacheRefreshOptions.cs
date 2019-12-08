using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum CacheRefreshOptions
	{
		None,
		SkipLocalCache,
		SkipXCache,
		SkipAllCache
	}
}