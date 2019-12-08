using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlockMeasurementEvent<T> : BlobOperationMeasurementEvent<T>, IBlobOperationWithRequestContentMeasurementEvent
	where T : BlockMeasurementEvent<T>
	{
		private long blockSize;

		private Microsoft.Cis.Services.Nephos.Common.Storage.BlobType type;

		public override Microsoft.Cis.Services.Nephos.Common.Storage.BlobType BlobType
		{
			get
			{
				return this.type;
			}
			set
			{
				this.type = value;
			}
		}

		[MeasurementEventParameter]
		public long BlockBytesWritten
		{
			get
			{
				return base.RequestBytesRead;
			}
		}

		[MeasurementEventParameter]
		public long BlockSize
		{
			get
			{
				return this.blockSize;
			}
			set
			{
				this.blockSize = value;
			}
		}

		public BlockMeasurementEvent(string operationName, Microsoft.Cis.Services.Nephos.Common.Storage.BlobType type) : base(operationName)
		{
			this.type = type;
		}
	}
}