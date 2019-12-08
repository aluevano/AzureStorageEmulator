using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlockCollection : IEnumerable<IBlock>, IEnumerable, IDisposable
	{
		int BlockCount
		{
			get;
		}
	}
}