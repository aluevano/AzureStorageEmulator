using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum AccessTierTransitionState
	{
		None = 0,
		RehydratingToHot = 1,
		RehydratingToCool = 2,
		Archiving = 4,
		ArchivingAndRehydratingToHot = 5,
		ArchivingAndRehydratingToCool = 6
	}
}