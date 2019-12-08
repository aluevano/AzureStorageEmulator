using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class GetAccountPropertiesMeasurementEvent : NephosBaseOperationMeasurementEvent<GetAccountPropertiesMeasurementEvent>, INephosBaseOperationMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		public GetAccountPropertiesMeasurementEvent() : base("GetAccountProperties")
		{
		}
	}
}