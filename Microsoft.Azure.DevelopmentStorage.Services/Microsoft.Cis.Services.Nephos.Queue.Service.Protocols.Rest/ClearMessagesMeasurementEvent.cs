using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class ClearMessagesMeasurementEvent : QueueOperationMeasurementEvent<ClearMessagesMeasurementEvent>
	{
		public ClearMessagesMeasurementEvent() : base("ClearMessages")
		{
		}
	}
}