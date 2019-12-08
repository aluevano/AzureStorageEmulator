using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Feb15RequestSettings : Feb14RequestSettings
	{
		public Feb15RequestSettings()
		{
			base.VersionString = "2015-02-21";
			base.AppendBlockApiEnabled = true;
			base.FilePreflightApiEnabled = true;
			base.SetFileServicePropertiesApiEnabled = true;
			base.SetDirectoryMetadataApiEnabled = true;
			base.SetShareAclApiEnabled = true;
			base.GetFileServicePropertiesApiEnabled = true;
			base.GetDirectoryMetadataApiEnabled = true;
			base.SetSharePropertiesApiEnabled = true;
			base.GetShareStatsApiEnabled = true;
			base.GetShareAclApiEnabled = true;
			base.CopyFileApiEnabled = true;
			base.AbortCopyFileApiEnabled = true;
		}
	}
}