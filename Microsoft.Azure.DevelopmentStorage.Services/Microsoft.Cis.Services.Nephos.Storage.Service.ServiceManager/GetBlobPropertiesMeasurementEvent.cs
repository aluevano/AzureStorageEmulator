using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobPropertiesMeasurementEvent : BlobContentMeasurementEvent<GetBlobPropertiesMeasurementEvent>
	{
		public GetBlobPropertiesMeasurementEvent() : base("GetBlobProperties")
		{
		}
	}
}