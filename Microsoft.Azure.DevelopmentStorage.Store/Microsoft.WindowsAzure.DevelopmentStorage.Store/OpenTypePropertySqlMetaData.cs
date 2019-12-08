using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class OpenTypePropertySqlMetaData
	{
		internal string ComparisonOperator
		{
			get;
			set;
		}

		internal string PropertyName
		{
			get;
			set;
		}

		internal int QueryIndex
		{
			get;
			set;
		}

		public OpenTypePropertySqlMetaData()
		{
		}
	}
}