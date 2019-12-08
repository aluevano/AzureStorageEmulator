using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public class ListQueuesResult : IListQueuesResultCollection, IEnumerable<IListQueuesResultQueueProperties>, IEnumerable
	{
		private IQueueContainerCollection queues;

		public string NextMarker
		{
			get
			{
				return this.queues.NextContainerStart;
			}
		}

		public ListQueuesResult(IQueueContainerCollection queues)
		{
			this.queues = queues;
		}

		public IEnumerator<IListQueuesResultQueueProperties> GetEnumerator()
		{
			foreach (IQueueContainer queueContainer in this.queues)
			{
				yield return new ListQueuesResultQueueProperties(queueContainer);
			}
		}

		IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
	}
}