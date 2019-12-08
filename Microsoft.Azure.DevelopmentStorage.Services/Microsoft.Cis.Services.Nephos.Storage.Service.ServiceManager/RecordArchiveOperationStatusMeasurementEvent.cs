using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class RecordArchiveOperationStatusMeasurementEvent : BlobContentMeasurementEvent<RecordArchiveOperationStatusMeasurementEvent>
	{
		public RecordArchiveOperationStatusMeasurementEvent() : base("RecordArchiveOperationStatus")
		{
		}
	}
}