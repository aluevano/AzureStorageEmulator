using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class ListMessagesMarker
	{
		public Guid NextMessageIdStart
		{
			get;
			set;
		}

		public string NextQueueStart
		{
			get;
			set;
		}

		public DateTime? NextVisibilityStart
		{
			get;
			set;
		}

		public int? SubQueueId
		{
			get;
			set;
		}

		public ListMessagesMarker()
		{
		}

		public ListMessagesMarker(string nextQueueStart, DateTime? nextVisibilityStart, Guid nextMessageIdStart) : this(nextQueueStart, 0, nextVisibilityStart, nextMessageIdStart)
		{
		}

		public ListMessagesMarker(string nextQueueStart, int subQueueId, DateTime? nextVisibilityStart, Guid nextMessageIdStart)
		{
			this.NextQueueStart = nextQueueStart;
			this.NextVisibilityStart = nextVisibilityStart;
			this.NextMessageIdStart = nextMessageIdStart;
			this.SubQueueId = new int?(subQueueId);
		}
	}
}