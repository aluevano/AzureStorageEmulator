using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class GetBlobMeasurementEvent : BlobContentMeasurementEvent<GetBlobMeasurementEvent>, IBlobOperationWithResponseContentMeasurementEvent
	{
		private long blobSize;

		private bool blobSizeKnown;

		[MeasurementEventParameter]
		public long BlobSize
		{
			get
			{
				return this.blobSize;
			}
			set
			{
				this.blobSize = value;
			}
		}

		public bool BlobSizeKnown
		{
			get
			{
				return this.blobSizeKnown;
			}
			set
			{
				this.blobSizeKnown = value;
			}
		}

		public GetBlobMeasurementEvent() : base("GetBlob")
		{
		}
	}
}