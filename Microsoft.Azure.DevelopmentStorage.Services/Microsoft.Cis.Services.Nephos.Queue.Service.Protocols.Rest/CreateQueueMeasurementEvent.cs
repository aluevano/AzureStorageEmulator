using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class CreateQueueMeasurementEvent : QueueOperationMeasurementEvent<CreateQueueMeasurementEvent>
	{
		public CreateQueueMeasurementEvent() : base("CreateQueue")
		{
		}
	}
}