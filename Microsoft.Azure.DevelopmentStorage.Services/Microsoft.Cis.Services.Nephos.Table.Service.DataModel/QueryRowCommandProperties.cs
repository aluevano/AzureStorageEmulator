using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public class QueryRowCommandProperties
	{
		public string HighKey
		{
			get;
			set;
		}

		public bool IsGetRowCommand
		{
			get;
			set;
		}

		public string LowKey
		{
			get;
			set;
		}

		public QueryRowCommandProperties(bool isGetRowCommand = false, string lowKey = null, string highKey = null)
		{
			this.IsGetRowCommand = isGetRowCommand;
			this.LowKey = lowKey;
			this.HighKey = highKey;
		}
	}
}