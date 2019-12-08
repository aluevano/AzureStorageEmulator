using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Feb14RequestSettings : Aug13RequestSettings
	{
		public Feb14RequestSettings()
		{
			base.VersionString = "2014-02-14";
			base.SetFilePropertiesApiEnabled = true;
			base.SetFileMetadataApiEnabled = true;
			base.SetShareMetadataApiEnabled = true;
			base.RenewFileLeaseApiEnabled = true;
			base.RenewShareLeaseApiEnabled = true;
			base.ReleaseFileLeaseApiEnabled = true;
			base.ReleaseShareLeaseApiEnabled = true;
			base.PutRangeApiEnabled = true;
			base.PutFileApiEnabled = true;
			base.ListRangesApiEnabled = true;
			base.ListFilesApiEnabled = true;
			base.ListSharesApiEnabled = true;
			base.GetFilePropertiesApiEnabled = true;
			base.GetFileMetadataApiEnabled = true;
			base.GetFileApiEnabled = true;
			base.GetDirectoryPropertiesApiEnabled = true;
			base.GetSharePropertiesApiEnabled = true;
			base.GetShareMetadataApiEnabled = true;
			base.DeleteFileApiEnabled = true;
			base.DeleteDirectoryApiEnabled = true;
			base.CreateDirectoryApiEnabled = true;
			base.DeleteShareApiEnabled = true;
			base.CreateShareApiEnabled = true;
			base.ClearRangeApiEnabled = true;
			base.ChangeFileLeaseApiEnabled = true;
			base.ChangeShareLeaseApiEnabled = true;
			base.BreakFileLeaseApiEnabled = true;
			base.BreakShareLeaseApiEnabled = true;
			base.AcquireFileLeaseApiEnabled = true;
			base.AcquireShareLeaseApiEnabled = true;
		}
	}
}