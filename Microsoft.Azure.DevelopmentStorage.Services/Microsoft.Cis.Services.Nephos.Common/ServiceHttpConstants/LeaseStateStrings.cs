using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class LeaseStateStrings
	{
		public const string Available = "available";

		public const string Leased = "leased";

		public const string Expired = "expired";

		public const string Breaking = "breaking";

		public const string Broken = "broken";

		public readonly static string[] LeaseStates;

		static LeaseStateStrings()
		{
			string[] strArrays = new string[] { "available", "leased", "expired", "breaking", "broken" };
			LeaseStateStrings.LeaseStates = strArrays;
		}
	}
}