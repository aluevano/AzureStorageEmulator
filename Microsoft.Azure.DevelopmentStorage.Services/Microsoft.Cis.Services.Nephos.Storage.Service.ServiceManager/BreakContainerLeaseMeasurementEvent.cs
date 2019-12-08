using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BreakContainerLeaseMeasurementEvent : BlobOperationMeasurementEvent<BreakContainerLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public BreakContainerLeaseMeasurementEvent() : base("BreakContainerLease")
		{
		}
	}
}