using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public class ListQueuesResultQueueProperties : IListQueuesResultQueueProperties
	{
		private string queueName;

		private NameValueCollection metadata;

		public NameValueCollection Metadata
		{
			get
			{
				return this.metadata;
			}
		}

		public string QueueName
		{
			get
			{
				return this.queueName;
			}
		}

		public ListQueuesResultQueueProperties(IQueueContainer queue)
		{
			if (queue == null)
			{
				throw new ArgumentNullException("queue");
			}
			this.queueName = queue.ContainerName;
			if (queue.ApplicationMetadata != null)
			{
				this.metadata = QueueHelpers.DeserializeMetadata(queue.ApplicationMetadata);
			}
		}
	}
}