using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Jul09RequestSettings : Apr09RequestSettings
	{
		public Jul09RequestSettings()
		{
			base.VersionString = "2009-07-17";
			base.UseNewPutBlockListImplementation = true;
		}
	}
}