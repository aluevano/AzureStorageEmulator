using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ChangeBlobLeaseMeasurementEvent : BlobOperationMeasurementEvent<ChangeBlobLeaseMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ChangeBlobLeaseMeasurementEvent() : base("ChangeBlobLease")
		{
		}
	}
}