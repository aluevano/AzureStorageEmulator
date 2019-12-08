using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class PutPageMeasurementEvent : BlobOperationMeasurementEvent<PutPageMeasurementEvent>, IBlobOperationWithRequestContentMeasurementEvent
	{
		private long dataSize;

		[MeasurementEventParameter]
		public long DataSize
		{
			get
			{
				return this.dataSize;
			}
			set
			{
				this.dataSize = value;
			}
		}

		[MeasurementEventParameter]
		public long PageBytesWritten
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

		public PutPageMeasurementEvent() : base("PutPage")
		{
		}
	}
}