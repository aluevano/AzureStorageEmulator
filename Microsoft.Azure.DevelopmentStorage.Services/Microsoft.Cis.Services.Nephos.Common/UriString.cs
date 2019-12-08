using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class UriString
	{
		public string RawString
		{
			get;
			private set;
		}

		public string SafeStringForLogging
		{
			get;
			private set;
		}

		public UriString(string rawUriString)
		{
			if (string.IsNullOrEmpty(rawUriString))
			{
				throw new ArgumentException("rawUriString", "Cannot be null or empty");
			}
			this.RawString = rawUriString;
			this.SafeStringForLogging = HttpUtilities.GetSafeUriString(rawUriString);
		}
	}
}