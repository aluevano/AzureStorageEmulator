using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	[Flags]
	public enum SasType
	{
		None,
		ResourceSas,
		AccountSas
	}
}