using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class SetQueueServicePropertiesMeasurementEvent : QueueOperationMeasurementEvent<SetQueueServicePropertiesMeasurementEvent>
	{
		public SetQueueServicePropertiesMeasurementEvent() : base("SetQueueServiceProperties")
		{
		}
	}
}