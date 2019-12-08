using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.QueueManager.XAC
{
	public interface IListQueuesResultCollection : IEnumerable<IListQueuesResultQueueProperties>, IEnumerable
	{
		string NextMarker
		{
			get;
		}
	}
}