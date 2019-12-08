using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlock
	{
		byte[] Identifier
		{
			get;
		}

		long Length
		{
			get;
		}
	}
}