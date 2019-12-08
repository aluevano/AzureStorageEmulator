using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class DeleteQueueMeasurementEvent : QueueOperationMeasurementEvent<DeleteQueueMeasurementEvent>
	{
		public DeleteQueueMeasurementEvent() : base("DeleteQueue")
		{
		}
	}
}