using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class PeekMessageMeasurementEvent : GetMessageMeasurementEvent
	{
		public PeekMessageMeasurementEvent() : base("PeekMessage")
		{
			base.OperationType = Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType.Peek;
		}
	}
}