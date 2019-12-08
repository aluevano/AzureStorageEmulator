using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetContainerMetadataMeasurementEvent : BlobOperationMeasurementEvent<GetContainerMetadataMeasurementEvent>
	{
		public GetContainerMetadataMeasurementEvent() : base("GetContainerMetadata")
		{
		}
	}
}