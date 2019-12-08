using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Oct16RequestSettings : May16RequestSettings
	{
		public Oct16RequestSettings()
		{
			base.VersionString = "2016-10-16";
			base.SnapshotShareApiEnabled = true;
			base.GetPostMigrationFileInfoApiEnabled = true;
			base.SetPostMigrationFileInfoApiEnabled = true;
		}
	}
}