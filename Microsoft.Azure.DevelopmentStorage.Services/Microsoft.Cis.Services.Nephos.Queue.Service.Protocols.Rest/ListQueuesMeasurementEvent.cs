using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class ListQueuesMeasurementEvent : QueueOperationMeasurementEvent<ListQueuesMeasurementEvent>
	{
		public ListQueuesMeasurementEvent() : base("ListQueues")
		{
		}
	}
}