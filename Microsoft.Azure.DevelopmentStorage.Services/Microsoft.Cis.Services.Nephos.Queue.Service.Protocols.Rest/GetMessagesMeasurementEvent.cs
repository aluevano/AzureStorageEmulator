using System;

namespace Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest
{
	public class GetMessagesMeasurementEvent : GetMessageMeasurementEvent
	{
		public GetMessagesMeasurementEvent() : base("GetMessages")
		{
			base.OperationType = Microsoft.Cis.Services.Nephos.Queue.Service.Protocols.Rest.OperationType.Get;
		}
	}
}