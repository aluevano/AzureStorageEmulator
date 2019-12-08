using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IGetPageRangeListResult
	{
		DateTime BlobLastModificationTime
		{
			get;
			set;
		}

		long ContentLength
		{
			get;
			set;
		}

		IPageRangeCollection PageRangeCollection
		{
			get;
		}
	}
}