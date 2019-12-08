using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ReleaseBlobLeaseMeasurementEvent : BlobOperationMeasurementEvent<ReleaseBlobLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ReleaseBlobLeaseMeasurementEvent() : base("ReleaseBlobLease")
		{
		}
	}
}