using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class PingMeasurementEvent : NephosBaseOperationMeasurementEvent<PingMeasurementEvent>, INephosBaseOperationMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		public static string OpName;

		static PingMeasurementEvent()
		{
			PingMeasurementEvent.OpName = "Ping";
		}

		public PingMeasurementEvent() : base(PingMeasurementEvent.OpName)
		{
		}
	}
}