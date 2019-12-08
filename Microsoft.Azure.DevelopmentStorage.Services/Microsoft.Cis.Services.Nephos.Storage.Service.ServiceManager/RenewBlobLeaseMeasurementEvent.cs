using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class RenewBlobLeaseMeasurementEvent : BlobOperationMeasurementEvent<RenewBlobLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public RenewBlobLeaseMeasurementEvent() : base("RenewBlobLease")
		{
		}
	}
}