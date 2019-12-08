using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class DeleteBlobMeasurementEvent : BlobOperationMeasurementEvent<DeleteBlobMeasurementEvent>
	{
		public DeleteBlobMeasurementEvent() : base("DeleteBlob")
		{
		}
	}
}