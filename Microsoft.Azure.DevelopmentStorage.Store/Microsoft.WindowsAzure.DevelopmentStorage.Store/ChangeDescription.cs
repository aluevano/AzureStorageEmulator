using Microsoft.UtilityComputing;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class ChangeDescription
	{
		public string AccountName
		{
			get;
			set;
		}

		public DateTime? EtagConditionUsed
		{
			get;
			set;
		}

		internal UtilityRow ExistingRow
		{
			get;
			set;
		}

		public bool IfMatchHeaderMissing
		{
			get;
			set;
		}

		public IQueryable QueryableRow
		{
			get;
			set;
		}

		public object Row
		{
			get;
			set;
		}

		public string TableName
		{
			get;
			set;
		}

		public UpdateKind UpdateType
		{
			get;
			set;
		}

		public ChangeDescription(string accountName, string tableName)
		{
			this.AccountName = accountName;
			this.TableName = tableName;
			this.EtagConditionUsed = null;
		}
	}
}