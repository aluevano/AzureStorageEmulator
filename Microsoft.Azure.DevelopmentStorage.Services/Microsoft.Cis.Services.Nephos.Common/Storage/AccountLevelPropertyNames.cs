using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum AccountLevelPropertyNames : short
	{
		None = 0,
		All = 32,
		SecretKeys = 32
	}
}