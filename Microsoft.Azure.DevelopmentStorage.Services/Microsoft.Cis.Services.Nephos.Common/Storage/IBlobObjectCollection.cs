using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBlobObjectCollection : IEnumerable<IBlobObject>, IEnumerable, IDisposable
	{
		bool HasMoreRows
		{
			get;
		}

		bool IsListingByAccount
		{
			get;
		}

		string NextBlobStart
		{
			get;
		}

		string NextContainerStart
		{
			get;
		}

		DateTime? NextSnapshotStart
		{
			get;
		}
	}
}