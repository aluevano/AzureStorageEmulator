using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AcquireBlobLeaseMeasurementEvent : BlobOperationMeasurementEvent<AcquireBlobLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public AcquireBlobLeaseMeasurementEvent() : base("AcquireBlobLease")
		{
		}
	}
}