using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class ListRowSummaryMarker
	{
		public string NextPartitionKeyStart
		{
			get;
			set;
		}

		public string NextRowKeyStart
		{
			get;
			set;
		}

		public string NextTableNameStart
		{
			get;
			set;
		}

		public ListRowSummaryMarker()
		{
		}

		public ListRowSummaryMarker(string nextTableNameStart, string nextPartitionKeyStart, string nextRowKeyStart)
		{
			this.NextTableNameStart = nextTableNameStart;
			this.NextPartitionKeyStart = nextPartitionKeyStart;
			this.NextRowKeyStart = nextRowKeyStart;
		}
	}
}