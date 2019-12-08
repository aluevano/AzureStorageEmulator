using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetBlobServicePropertiesMeasurementEvent : BlobOperationMeasurementEvent<SetBlobServicePropertiesMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		public SetBlobServicePropertiesMeasurementEvent() : base("SetBlobServiceProperties")
		{
		}
	}
}