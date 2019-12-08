using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum SasProtocol
	{
		None,
		Http,
		Https,
		All
	}
}