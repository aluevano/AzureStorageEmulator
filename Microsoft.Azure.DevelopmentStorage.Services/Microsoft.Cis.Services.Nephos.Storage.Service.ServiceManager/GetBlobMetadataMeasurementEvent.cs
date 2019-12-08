using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobMetadataMeasurementEvent : BlobOperationMeasurementEvent<GetBlobMetadataMeasurementEvent>
	{
		public GetBlobMetadataMeasurementEvent() : base("GetBlobMetadata")
		{
		}
	}
}