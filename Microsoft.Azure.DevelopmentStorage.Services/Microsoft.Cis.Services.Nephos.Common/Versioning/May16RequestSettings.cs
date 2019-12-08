using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class May16RequestSettings : Feb16RequestSettings
	{
		public May16RequestSettings()
		{
			base.VersionString = "2016-05-31";
			base.IncrementalCopyBlobApiEnabled = true;
		}
	}
}