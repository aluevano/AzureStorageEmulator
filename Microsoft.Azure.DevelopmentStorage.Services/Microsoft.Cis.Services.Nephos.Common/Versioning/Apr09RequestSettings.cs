using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Apr09RequestSettings : Oct08RequestSettings
	{
		public Apr09RequestSettings()
		{
			base.VersionString = "2009-04-14";
			base.CopyBlobApiEnabled = true;
			base.TableBatchApiEnabled = true;
			base.UseNewGetBlockListImplementation = true;
		}
	}
}