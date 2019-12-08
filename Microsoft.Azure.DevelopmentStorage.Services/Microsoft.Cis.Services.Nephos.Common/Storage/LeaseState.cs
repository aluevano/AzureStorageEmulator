using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum LeaseState
	{
		Available,
		Leased,
		Expired,
		Breaking,
		Broken
	}
}