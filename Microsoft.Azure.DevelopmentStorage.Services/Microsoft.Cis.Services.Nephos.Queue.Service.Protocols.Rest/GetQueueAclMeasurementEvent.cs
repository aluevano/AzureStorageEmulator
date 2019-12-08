using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetQueueAclMeasurementEvent : QueueOperationMeasurementEvent<GetQueueAclMeasurementEvent>
	{
		public GetQueueAclMeasurementEvent() : base("GetQueueAcl")
		{
		}
	}
}