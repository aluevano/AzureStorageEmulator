using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetBlobMetadataMeasurementEvent : BlobOperationMeasurementEvent<SetBlobMetadataMeasurementEvent>
	{
		public SetBlobMetadataMeasurementEvent() : base("SetBlobMetadata")
		{
		}
	}
}