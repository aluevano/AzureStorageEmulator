using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetQueueMetadataMeasurementEvent : QueueOperationMeasurementEvent<GetQueueMetadataMeasurementEvent>
	{
		public GetQueueMetadataMeasurementEvent() : base("GetQueueMetadata")
		{
		}
	}
}