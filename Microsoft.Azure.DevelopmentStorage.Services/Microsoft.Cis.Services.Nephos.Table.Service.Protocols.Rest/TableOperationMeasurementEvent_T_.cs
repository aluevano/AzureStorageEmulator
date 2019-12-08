using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using System;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableOperationMeasurementEvent<T> : NephosBaseOperationMeasurementEvent<T>, ITableOperationMeasurementEvent, ITableMeasurementEvent, INephosBaseMeasurementEvent, IMeasurementEvent, IDisposable
	where T : TableOperationMeasurementEvent<T>
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

		protected TableOperationMeasurementEvent(string operationName) : base(operationName)
		{
		}

		public override string GetObjectKey()
		{
			if (string.IsNullOrEmpty(this.RealTableName))
			{
				return base.GenerateObjectKeyFrom(new string[] { base.AccountName });
			}
			if (string.IsNullOrEmpty(this.PartitionKey))
			{
				string[] accountName = new string[] { base.AccountName, this.RealTableName };
				return base.GenerateObjectKeyFrom(accountName);
			}
			if (string.IsNullOrEmpty(this.RowKey))
			{
				string[] strArrays = new string[] { base.AccountName, this.RealTableName, this.PartitionKey };
				return base.GenerateObjectKeyFrom(strArrays);
			}
			string[] accountName1 = new string[] { base.AccountName, this.RealTableName, this.PartitionKey, this.RowKey };
			return base.GenerateObjectKeyFrom(accountName1);
		}
	}
}