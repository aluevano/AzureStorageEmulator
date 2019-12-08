using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Flags]
	public enum ContainerPropertyNames
	{
		Default = 0,
		None = 0,
		LastModificationTime = 2,
		ServiceMetadata = 8,
		ApplicationMetadata = 16,
		LeaseType = 128,
		All = 154,
		Common = 154
	}
}