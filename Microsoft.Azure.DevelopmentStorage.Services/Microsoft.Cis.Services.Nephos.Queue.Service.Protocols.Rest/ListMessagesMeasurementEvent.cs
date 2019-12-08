using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class ListMessagesMeasurementEvent : QueueOperationMeasurementEvent<ListMessagesMeasurementEvent>
	{
		public ListMessagesMeasurementEvent() : base("ListMessages")
		{
		}
	}
}