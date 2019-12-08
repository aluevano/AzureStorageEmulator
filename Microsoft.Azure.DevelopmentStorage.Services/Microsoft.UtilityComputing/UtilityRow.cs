using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Table.Service.Providers.XTable;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.UtilityComputing
{
	[DataServiceKey(new string[] { "PartitionKey", "RowKey" })]
	[ETag("Timestamp")]
	public class UtilityRow
	{
		public const int MaxPartitionKeyLength = 1024;

		public const int MaxRowKeyLength = 1024;

		private string m_partitionKey;

		private string m_rowKey;

		private Dictionary<string, object> m_columnValues;

		public IDictionary<string, object> ColumnValues
		{
			get
			{
				return this.m_columnValues;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public object this[string columnName]
		{
			get
			{
				object obj;
				if (string.Equals(columnName, "PartitionKey"))
				{
					return this.PartitionKey;
				}
				if (string.Equals(columnName, "RowKey"))
				{
					return this.RowKey;
				}
				if (string.Equals(columnName, "Timestamp"))
				{
					return this.Timestamp;
				}
				if (!this.ColumnValues.TryGetValue(columnName, out obj))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { columnName };
					throw new XStoreArgumentException(string.Format(invariantCulture, "Invalid column name: '{0}'", objArray));
				}
				return obj;
			}
			set
			{
				if (string.Equals(columnName, "PartitionKey"))
				{
					this.PartitionKey = (string)value;
					return;
				}
				if (string.Equals(columnName, "RowKey"))
				{
					this.RowKey = (string)value;
					return;
				}
				if (string.Equals(columnName, "Timestamp"))
				{
					this.Timestamp = (DateTime)value;
					return;
				}
				this.ColumnValues[columnName] = value;
			}
		}

		public string PartitionKey
		{
			get
			{
				return this.m_partitionKey;
			}
			set
			{
				TableDataContextHelper.ValidateKeyValue(value);
				this.m_partitionKey = value;
			}
		}

		public string RowKey
		{
			get
			{
				return this.m_rowKey;
			}
			set
			{
				TableDataContextHelper.ValidateKeyValue(value);
				this.m_rowKey = value;
			}
		}

		public DateTime Timestamp
		{
			get;
			set;
		}

		public UtilityRow()
		{
			this.m_columnValues = new Dictionary<string, object>();
		}

		public UtilityRow(string partitionKey, DateTime timestamp, IDictionary<string, object> columnValues)
		{
			this.PartitionKey = partitionKey;
			this.Timestamp = timestamp;
			this.m_columnValues = new Dictionary<string, object>(columnValues);
		}

		public UtilityRow Clone()
		{
			UtilityRow utilityRow = new UtilityRow()
			{
				PartitionKey = this.PartitionKey,
				RowKey = this.RowKey,
				Timestamp = this.Timestamp
			};
			foreach (KeyValuePair<string, object> columnValue in this.ColumnValues)
			{
				utilityRow.ColumnValues.Add(columnValue.Key, columnValue.Value);
			}
			return utilityRow;
		}
	}
}