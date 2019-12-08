using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class RequestRedirectionMeasurementEvent : NephosBaseOperationMeasurementEvent<RequestRedirectionMeasurementEvent>, INephosBaseOperationMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		public static string OpName;

		static RequestRedirectionMeasurementEvent()
		{
			RequestRedirectionMeasurementEvent.OpName = "RequestRedirection";
		}

		public RequestRedirectionMeasurementEvent() : base(RequestRedirectionMeasurementEvent.OpName)
		{
		}
	}
}