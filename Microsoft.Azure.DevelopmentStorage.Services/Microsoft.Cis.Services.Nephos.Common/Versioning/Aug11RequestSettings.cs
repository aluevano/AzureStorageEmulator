using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Aug11RequestSettings : Sep09RequestSettings
	{
		public Aug11RequestSettings()
		{
			base.VersionString = "2011-08-18";
			base.UpdateMessageApiEnabled = true;
		}
	}
}