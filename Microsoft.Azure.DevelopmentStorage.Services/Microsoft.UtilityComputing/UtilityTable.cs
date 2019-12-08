using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Data.Services.Common;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Microsoft.UtilityComputing
{
	[DataServiceKey("TableName")]
	public class UtilityTable
	{
		[SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
		public object this[string columnName]
		{
			get
			{
				if (string.Equals(columnName, "TableName"))
				{
					return this.TableName;
				}
				if (string.Equals(columnName, "RequestedIOPS"))
				{
					return this.RequestedIOPS;
				}
				if (string.Equals(columnName, "ProvisionedIOPS"))
				{
					return this.ProvisionedIOPS;
				}
				if (!string.Equals(columnName, "TableStatus"))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { columnName };
					throw new XStoreArgumentException(string.Format(invariantCulture, "Invalid column name: '{0}'", objArray));
				}
				return this.TableStatus;
			}
			set
			{
				if (string.Equals(columnName, "TableName"))
				{
					this.TableName = (string)value;
					return;
				}
				if (!string.Equals(columnName, "RequestedIOPS"))
				{
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { columnName };
					throw new XStoreArgumentException(string.Format(invariantCulture, "Invalid column name: '{0}'", objArray));
				}
				int num = 0;
				try
				{
					num = Convert.ToInt32(value);
				}
				catch (Exception exception)
				{
					throw new XStoreArgumentException(exception.Message);
				}
				this.RequestedIOPS = num;
			}
		}

		internal int ProvisionedIOPS
		{
			get;
			set;
		}

		internal int RequestedIOPS
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		internal string TableStatus
		{
			get;
			set;
		}

		internal string VersionedTableName
		{
			get;
			set;
		}

		public UtilityTable()
		{
		}
	}
}