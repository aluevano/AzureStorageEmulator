using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class RenewContainerLeaseMeasurementEvent : BlobOperationMeasurementEvent<RenewContainerLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public RenewContainerLeaseMeasurementEvent() : base("RenewContainerLease")
		{
		}
	}
}