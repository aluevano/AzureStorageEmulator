using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobServiceStatsMeasurementEvent : BlobOperationMeasurementEvent<GetBlobServiceStatsMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public GetBlobServiceStatsMeasurementEvent() : base("GetBlobServiceStats")
		{
		}
	}
}