using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Feb12RequestSettings : Aug11RequestSettings
	{
		public Feb12RequestSettings()
		{
			base.VersionString = "2012-02-12";
			base.AbortCopyBlobApiEnabled = true;
			base.ChangeBlobLeaseApiEnabled = true;
			base.AcquireContainerLeaseApiEnabled = true;
			base.BreakContainerLeaseApiEnabled = true;
			base.ChangeContainerLeaseApiEnabled = true;
			base.RenewContainerLeaseApiEnabled = true;
			base.ReleaseContainerLeaseApiEnabled = true;
			base.GetQueueAclApiEnabled = true;
			base.SetQueueAclApiEnabled = true;
			base.SetTableAclApiEnabled = true;
			base.GetTableAclApiEnabled = true;
		}
	}
}