using System;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Flags]
	public enum SasResourceType
	{
		None = 0,
		Service = 1,
		Container = 2,
		Object = 4
	}
}