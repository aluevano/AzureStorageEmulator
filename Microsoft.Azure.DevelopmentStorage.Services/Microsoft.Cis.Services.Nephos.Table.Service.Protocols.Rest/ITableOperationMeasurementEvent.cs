using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	internal interface ITableOperationMeasurementEvent : ITableMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		long RequestBytesRead
		{
			get;
			set;
		}

		long ResponseBytesWritten
		{
			get;
			set;
		}
	}
}