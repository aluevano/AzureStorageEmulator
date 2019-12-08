using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ListRowSummaryResult : SummaryResult
	{
		public long TotalContentSize
		{
			get;
			set;
		}

		public ListRowSummaryResult()
		{
		}

		public ListRowSummaryResult(long rowCount, long totalContentSize, string nextMarker) : base(rowCount, (long)0, nextMarker)
		{
			this.TotalContentSize = totalContentSize;
		}
	}
}