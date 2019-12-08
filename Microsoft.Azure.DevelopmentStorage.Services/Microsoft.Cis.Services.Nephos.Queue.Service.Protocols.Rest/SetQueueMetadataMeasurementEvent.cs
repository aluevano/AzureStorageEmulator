using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class SetQueueMetadataMeasurementEvent : QueueOperationMeasurementEvent<SetQueueMetadataMeasurementEvent>
	{
		public SetQueueMetadataMeasurementEvent() : base("SetQueueMetadata")
		{
		}
	}
}