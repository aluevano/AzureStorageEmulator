using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ReleaseContainerLeaseMeasurementEvent : BlobOperationMeasurementEvent<ReleaseContainerLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ReleaseContainerLeaseMeasurementEvent() : base("ReleaseContainerLease")
		{
		}
	}
}