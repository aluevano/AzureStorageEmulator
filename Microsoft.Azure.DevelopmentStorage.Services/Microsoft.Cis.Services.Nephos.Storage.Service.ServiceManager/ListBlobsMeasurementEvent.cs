using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class ListBlobsMeasurementEvent : BlobOperationMeasurementEvent<ListBlobsMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public ListBlobsMeasurementEvent() : base("ListBlobs")
		{
		}
	}
}