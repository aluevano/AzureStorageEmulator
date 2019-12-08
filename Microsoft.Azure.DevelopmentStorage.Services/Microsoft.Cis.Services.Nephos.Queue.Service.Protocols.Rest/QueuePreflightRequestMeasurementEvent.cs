using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class QueuePreflightRequestMeasurementEvent : QueueOperationMeasurementEvent<QueuePreflightRequestMeasurementEvent>
	{
		public QueuePreflightRequestMeasurementEvent() : base("QueuePreflightRequest")
		{
		}
	}
}