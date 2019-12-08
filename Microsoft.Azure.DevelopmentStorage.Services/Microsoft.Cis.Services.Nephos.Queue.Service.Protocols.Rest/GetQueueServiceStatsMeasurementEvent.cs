using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetQueueServiceStatsMeasurementEvent : QueueOperationMeasurementEvent<GetQueueServiceStatsMeasurementEvent>
	{
		public GetQueueServiceStatsMeasurementEvent() : base("GetQueueServiceStats")
		{
		}
	}
}