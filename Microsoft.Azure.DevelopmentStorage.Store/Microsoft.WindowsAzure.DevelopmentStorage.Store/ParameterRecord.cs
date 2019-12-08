using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class ParameterRecord
	{
		public bool IsStringValue
		{
			get;
			set;
		}

		public object Value
		{
			get;
			set;
		}

		public ParameterRecord(object value, bool isStringValue = false)
		{
			this.Value = value;
			this.IsStringValue = isStringValue;
		}
	}
}