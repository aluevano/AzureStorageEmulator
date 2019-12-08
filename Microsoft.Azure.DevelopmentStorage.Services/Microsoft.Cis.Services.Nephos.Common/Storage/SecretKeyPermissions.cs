using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum SecretKeyPermissions
	{
		None = 0,
		Read = 1,
		Write = 2,
		Delete = 4,
		Full = 268435455
	}
}