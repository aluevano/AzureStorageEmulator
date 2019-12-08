using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum SasService
	{
		None = 0,
		Blob = 1,
		File = 2,
		Queue = 4,
		Table = 8
	}
}