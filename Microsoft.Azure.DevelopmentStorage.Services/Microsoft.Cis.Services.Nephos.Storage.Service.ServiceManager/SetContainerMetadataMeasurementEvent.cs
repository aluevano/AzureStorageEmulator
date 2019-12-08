using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SetContainerMetadataMeasurementEvent : BlobOperationMeasurementEvent<SetContainerMetadataMeasurementEvent>
	{
		public SetContainerMetadataMeasurementEvent() : base("SetContainerMetadata")
		{
		}
	}
}