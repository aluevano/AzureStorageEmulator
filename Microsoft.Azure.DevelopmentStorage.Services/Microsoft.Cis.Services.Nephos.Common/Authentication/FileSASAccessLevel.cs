using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum FileSASAccessLevel
	{
		None,
		File,
		Share
	}
}