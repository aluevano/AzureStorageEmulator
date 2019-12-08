using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Sep09RequestSettings : Jul09RequestSettings
	{
		public Sep09RequestSettings()
		{
			base.VersionString = "2009-09-19";
			base.SetBlobPropertiesApiEnabled = true;
			base.SnapshotBlobApiEnabled = true;
			base.PutPageApiEnabled = true;
			base.ClearPageApiEnabled = true;
			base.GetPageRangesApiEnabled = true;
			base.AcquireBlobLeaseApiEnabled = true;
			base.RenewBlobLeaseApiEnabled = true;
			base.ReleaseBlobLeaseApiEnabled = true;
			base.BreakBlobLeaseApiEnabled = true;
			base.SetBlobServicePropertiesApiEnabled = true;
			base.GetBlobServicePropertiesApiEnabled = true;
			base.SetQueueServicePropertiesApiEnabled = true;
			base.GetQueueServicePropertiesApiEnabled = true;
			base.SetTableServicePropertiesApiEnabled = true;
			base.GetTableServicePropertiesApiEnabled = true;
		}
	}
}