using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BreakBlobLeaseMeasurementEvent : BlobOperationMeasurementEvent<BreakBlobLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public BreakBlobLeaseMeasurementEvent() : base("BreakBlobLease")
		{
		}
	}
}