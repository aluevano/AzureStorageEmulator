using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetBlobPropertiesMeasurementEvent : BlobContentMeasurementEvent<SetBlobPropertiesMeasurementEvent>
	{
		public SetBlobPropertiesMeasurementEvent() : base("SetBlobProperties")
		{
		}
	}
}