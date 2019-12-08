using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class ExceptionExtensions
	{
		public static string GetLogString(this Exception exception)
		{
			string empty = string.Empty;
			empty = (false ? string.Concat(exception.GetType().FullName, " : ", exception.Message) : exception.ToString());
			return empty;
		}
	}
}