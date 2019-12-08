using System;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class Jul13RequestSettings : Feb12RequestSettings
	{
		public Jul13RequestSettings()
		{
			base.VersionString = "2013-07-14";
			base.GetQueueServiceStatsApiEnabled = true;
			base.GetTableServiceStatsApiEnabled = true;
			base.GetBlobServiceStatsApiEnabled = true;
		}
	}
}