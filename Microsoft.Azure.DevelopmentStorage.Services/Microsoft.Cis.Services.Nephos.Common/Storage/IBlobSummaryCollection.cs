using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobSummaryCollection : IEnumerable<IBlobSummary>, IEnumerable, IDisposable
	{
		bool HasMoreRows
		{
			get;
		}

		string NextBlobStart
		{
			get;
		}
	}
}