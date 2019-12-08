using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	internal interface ITableMeasurementEvent : INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		string TableName
		{
			get;
			set;
		}
	}
}