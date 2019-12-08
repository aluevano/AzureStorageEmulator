using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IPageRangeCollection : IEnumerable<IPageRange>, IEnumerable, IDisposable
	{
		bool HasMoreRows
		{
			get;
		}

		long NextPageStart
		{
			get;
		}

		int PageRangeCount
		{
			get;
		}
	}
}