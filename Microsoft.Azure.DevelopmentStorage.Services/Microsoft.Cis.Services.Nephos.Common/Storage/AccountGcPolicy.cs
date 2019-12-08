using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum AccountGcPolicy
	{
		None = 0,
		DeleteImmediately = 1,
		DeleteImmediatelyAndPreserveAccountRow = 3
	}
}