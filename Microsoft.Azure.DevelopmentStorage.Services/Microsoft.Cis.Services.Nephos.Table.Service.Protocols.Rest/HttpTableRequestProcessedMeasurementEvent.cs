using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class HttpTableRequestProcessedMeasurementEvent : NephosBaseMeasurementEvent<HttpTableRequestProcessedMeasurementEvent>, ITableMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	{
		private string tableName;

		public override string OperationPartitionKey
		{
			get
			{
				return null;
			}
		}

		public string PartitionKey
		{
			get
			{
				return null;
			}
		}

		public string RealTableName
		{
			get
			{
				return null;
			}
		}

		public string RowKey
		{
			get
			{
				return null;
			}
		}

		[MeasurementEventParameter]
		public string TableName
		{
			get
			{
				return this.tableName;
			}
			set
			{
				this.tableName = value;
			}
		}

		public HttpTableRequestProcessedMeasurementEvent() : base("HttpTableRequestProcessed")
		{
		}
	}
}