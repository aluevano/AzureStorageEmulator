using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetQueueServicePropertiesMeasurementEvent : QueueOperationMeasurementEvent<GetQueueServicePropertiesMeasurementEvent>
	{
		public GetQueueServicePropertiesMeasurementEvent() : base("GetQueueServiceProperties")
		{
		}
	}
}