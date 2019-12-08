using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class PeekMessagesMeasurementEvent : GetMessageMeasurementEvent
	{
		public PeekMessagesMeasurementEvent() : base("PeekMessages")
		{
			base.OperationType = Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType.Peek;
		}
	}
}