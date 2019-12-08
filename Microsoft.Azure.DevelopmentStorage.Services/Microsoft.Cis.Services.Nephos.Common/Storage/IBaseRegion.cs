using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBaseRegion
	{
		long Length
		{
			get;
		}

		long Offset
		{
			get;
		}
	}
}