using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class SetQueueAclMeasurementEvent : QueueOperationMeasurementEvent<SetQueueAclMeasurementEvent>
	{
		public SetQueueAclMeasurementEvent() : base("SetQueueAcl")
		{
		}
	}
}