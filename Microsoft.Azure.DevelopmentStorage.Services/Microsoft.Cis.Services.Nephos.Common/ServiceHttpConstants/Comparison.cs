using Microsoft.Cis.Services.Nephos.Common;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class Comparison
	{
		public static bool StringContains(string mainString, string stringToSearchFor)
		{
			NephosAssertionException.Assert(mainString != null);
			NephosAssertionException.Assert(stringToSearchFor != null);
			if (string.IsNullOrEmpty(mainString))
			{
				return string.IsNullOrEmpty(stringToSearchFor);
			}
			return mainString.IndexOf(stringToSearchFor, StringComparison.OrdinalIgnoreCase) != -1;
		}

		public static bool StringEquals(string value1, string value2)
		{
			return string.Compare(value1, value2, StringComparison.Ordinal) == 0;
		}

		public static bool StringEqualsIgnoreCase(string value1, string value2)
		{
			return string.Compare(value1, value2, StringComparison.OrdinalIgnoreCase) == 0;
		}
	}
}