using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Flags]
	public enum EntryPointType : long
	{
		None = 0,
		SecureInterface = 1,
		HighPriority = 2,
		SlbInterface = 4,
		ServiceTunnelingInterface = 16,
		LocationInterface = 32,
		LocationAccountInterface = 64,
		ResourceManagerInterface = 256,
		DsrEnabledInterface = 512,
		Invalid = 8388608
	}
}