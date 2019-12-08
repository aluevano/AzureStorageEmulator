using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class AbortCopyBlobMeasurementEvent : BlobOperationMeasurementEvent<AbortCopyBlobMeasurementEvent>
	{
		public AbortCopyBlobMeasurementEvent() : base("AbortCopyBlob")
		{
		}
	}
}