using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AcquireContainerLeaseMeasurementEvent : BlobOperationMeasurementEvent<AcquireContainerLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public AcquireContainerLeaseMeasurementEvent() : base("AcquireContainerLease")
		{
		}
	}
}