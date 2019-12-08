using MeasurementEvents;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface INephosBaseOperationMeasurementEvent : INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		long MetadataKeySize
		{
			get;
			set;
		}

		long MetadataValueSize
		{
			get;
			set;
		}

		long RequestBytesRead
		{
			get;
			set;
		}

		long RequestHeaderBytesRead
		{
			get;
			set;
		}

		long ResponseBytesWritten
		{
			get;
			set;
		}

		long ResponseHeaderBytesWritten
		{
			get;
			set;
		}
	}
}