using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobServicePropertiesMeasurementEvent : BlobOperationMeasurementEvent<GetBlobServicePropertiesMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public GetBlobServicePropertiesMeasurementEvent() : base("GetBlobServiceProperties")
		{
		}
	}
}