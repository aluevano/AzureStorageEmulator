using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public class OperationInfo
	{
		public string AccountName
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Table.Service.DataModel.Resource Resource
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		public OperationInfo()
		{
		}

		public OperationInfo(string accountName, Microsoft.Cis.Services.Nephos.Table.Service.DataModel.Resource resource, string tableName)
		{
			this.AccountName = accountName;
			this.Resource = resource;
			this.TableName = tableName;
		}
	}
}