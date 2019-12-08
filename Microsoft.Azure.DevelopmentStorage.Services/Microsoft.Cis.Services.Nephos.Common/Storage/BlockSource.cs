using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public enum BlockSource
	{
		None,
		Uncommitted,
		Committed,
		Latest
	}
}