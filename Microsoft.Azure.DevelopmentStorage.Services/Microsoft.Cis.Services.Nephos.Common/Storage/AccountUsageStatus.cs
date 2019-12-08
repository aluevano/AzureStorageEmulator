using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum AccountUsageStatus
	{
		None,
		BelowQuota,
		AboveQuota
	}
}