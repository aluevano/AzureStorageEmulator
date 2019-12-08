using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ListBlobSummaryResult : SummaryResult
	{
		public string NextBlobName
		{
			get;
			set;
		}

		public string NextContainerName
		{
			get;
			set;
		}

		public long TotalContentSize
		{
			get;
			set;
		}

		public ListBlobSummaryResult()
		{
		}

		public ListBlobSummaryResult(long count, long totalContentSize, string nextContainerName, string nextBlobName) : base(count, (long)0, "")
		{
			this.TotalContentSize = totalContentSize;
			this.NextContainerName = nextContainerName;
			this.NextBlobName = nextBlobName;
		}
	}
}