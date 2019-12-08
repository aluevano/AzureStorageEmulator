using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutBlobMeasurementEvent : BlobContentMeasurementEvent<PutBlobMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent
	{
		private long blobSize;

		[MeasurementEventParameter]
		public long BlobBytesWritten
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

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

		public PutBlobMeasurementEvent() : base("PutBlob")
		{
		}
	}
}