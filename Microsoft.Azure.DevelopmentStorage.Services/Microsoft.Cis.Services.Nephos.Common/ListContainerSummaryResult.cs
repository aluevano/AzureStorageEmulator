using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class ListContainerSummaryResult : SummaryResult
	{
		public ListContainerSummaryResult()
		{
		}

		public ListContainerSummaryResult(long count, long totalMetadataSize, string nextMarker) : base(count, totalMetadataSize, nextMarker)
		{
		}
	}
}