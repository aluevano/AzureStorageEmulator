using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum AccountPermissions
	{
		Empty = 0,
		None = 2,
		Read = 4,
		Delete = 8,
		Write = 16,
		Full = 2147483644
	}
}