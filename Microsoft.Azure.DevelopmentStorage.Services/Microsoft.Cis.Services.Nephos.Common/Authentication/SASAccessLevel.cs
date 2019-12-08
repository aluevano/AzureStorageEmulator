using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum SASAccessLevel
	{
		None,
		Blob,
		Container
	}
}