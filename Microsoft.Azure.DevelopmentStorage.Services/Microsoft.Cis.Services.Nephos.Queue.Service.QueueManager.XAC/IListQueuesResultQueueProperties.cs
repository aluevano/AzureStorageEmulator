using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public interface IListQueuesResultQueueProperties
	{
		NameValueCollection Metadata
		{
			get;
		}

		string QueueName
		{
			get;
		}
	}
}