using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class RequestFailureMeasurementEvent<T> : NephosBaseOperationMeasurementEvent<T>, INephosBaseOperationMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : RequestFailureMeasurementEvent<T>
	{
		protected RequestFailureMeasurementEvent(string operationName) : base(operationName)
		{
		}
	}
}