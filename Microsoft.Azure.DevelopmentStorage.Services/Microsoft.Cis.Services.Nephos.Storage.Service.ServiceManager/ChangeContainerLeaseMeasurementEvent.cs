using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ChangeContainerLeaseMeasurementEvent : BlobOperationMeasurementEvent<ChangeContainerLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ChangeContainerLeaseMeasurementEvent() : base("ChangeContainerLease")
		{
		}
	}
}