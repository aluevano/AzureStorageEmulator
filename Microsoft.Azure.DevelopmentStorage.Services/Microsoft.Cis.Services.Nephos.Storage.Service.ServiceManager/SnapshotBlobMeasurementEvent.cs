using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class SnapshotBlobMeasurementEvent : BlobOperationMeasurementEvent<SnapshotBlobMeasurementEvent>
	{
		public SnapshotBlobMeasurementEvent() : base("SnapshotBlob")
		{
		}
	}
}