using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetContainerPropertiesMeasurementEvent : BlobOperationMeasurementEvent<GetContainerPropertiesMeasurementEvent>
	{
		public GetContainerPropertiesMeasurementEvent() : base("GetContainerProperties")
		{
		}
	}
}