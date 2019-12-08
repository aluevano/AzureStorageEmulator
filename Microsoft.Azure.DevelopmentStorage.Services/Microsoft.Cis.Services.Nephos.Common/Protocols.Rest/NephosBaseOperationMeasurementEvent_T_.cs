using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class NephosBaseOperationMeasurementEvent<T> : NephosBaseMeasurementEvent<T>, INephosBaseOperationMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : NephosBaseOperationMeasurementEvent<T>
	{
		private long metadataKeySize;

		private long metadataValueSize;

		private long requestBytesRead;

		private long responseBytesWritten;

		private long requestHeaderBytesRead;

		private long responseHeaderBytesWritten;

		[MeasurementEventParameter]
		public long MetadataKeySize
		{
			get
			{
				return this.metadataKeySize;
			}
			set
			{
				this.metadataKeySize = value;
			}
		}

		[MeasurementEventParameter]
		public long MetadataValueSize
		{
			get
			{
				return this.metadataValueSize;
			}
			set
			{
				this.metadataValueSize = value;
			}
		}

		[MeasurementEventParameter]
		public long RequestBytesRead
		{
			get
			{
				return this.requestBytesRead;
			}
			set
			{
				this.requestBytesRead = value;
			}
		}

		[MeasurementEventParameter]
		public long RequestHeaderBytesRead
		{
			get
			{
				return this.requestHeaderBytesRead;
			}
			set
			{
				this.requestHeaderBytesRead = value;
			}
		}

		[MeasurementEventParameter]
		public long ResponseBytesWritten
		{
			get
			{
				return this.responseBytesWritten;
			}
			set
			{
				this.responseBytesWritten = value;
			}
		}

		[MeasurementEventParameter]
		public long ResponseHeaderBytesWritten
		{
			get
			{
				return this.responseHeaderBytesWritten;
			}
			set
			{
				this.responseHeaderBytesWritten = value;
			}
		}

		protected NephosBaseOperationMeasurementEvent(string operationName) : base(operationName)
		{
		}
	}
}