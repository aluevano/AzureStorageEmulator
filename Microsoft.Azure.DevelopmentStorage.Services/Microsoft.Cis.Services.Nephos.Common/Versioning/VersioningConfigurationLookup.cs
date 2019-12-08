using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Versioning
{
	public class VersioningConfigurationLookup
	{
		public List<VersionedRequestSettings> AllVersions
		{
			get;
			private set;
		}

		public string DefaultVersion
		{
			get;
			private set;
		}

		private Dictionary<string, VersionedRequestSettings> EnabledVersions
		{
			get;
			set;
		}

		public static VersioningConfigurationLookup Instance
		{
			get;
			private set;
		}

		public string LatestVersion
		{
			get;
			private set;
		}

		static VersioningConfigurationLookup()
		{
			VersioningConfigurationLookup.Instance = new VersioningConfigurationLookup();
		}

		private VersioningConfigurationLookup()
		{
			this.AllVersions = new List<VersionedRequestSettings>();
			this.EnabledVersions = new Dictionary<string, VersionedRequestSettings>();
			this.AddVersion(new Oct08RequestSettings(), false);
			this.AddVersion(new Apr09RequestSettings(), false);
			this.AddVersion(new Jul09RequestSettings(), false);
			this.AddVersion(new Sep09RequestSettings(), false);
			this.AddVersion(new Aug11RequestSettings(), false);
			this.AddVersion(new Feb12RequestSettings(), false);
			this.AddVersion(new Aug13RequestSettings(), false);
			this.AddVersion(new Feb14RequestSettings(), false);
			this.AddVersion(new Feb15RequestSettings(), false);
			this.AddVersion(new Apr15RequestSettings(), false);
			this.AddVersion(new Jul15RequestSettings(), false);
			this.AddVersion(new Dec15RequestSettings(), false);
			this.AddVersion(new May16RequestSettings(), false);
			this.AddVersion(new Oct16RequestSettings(), false);
			this.AddVersion(new Apr17RequestSettings(), false);
			this.DefaultVersion = "2008-10-27";
			this.LatestVersion = "2017-04-17";
		}

		private void AddVersion(VersionedRequestSettings versionedSettings, bool isInternal = false)
		{
			if (versionedSettings == null)
			{
				throw new ArgumentNullException("versionedSettings");
			}
			this.AllVersions.Add(versionedSettings);
			this.EnabledVersions.Add(versionedSettings.VersionString, versionedSettings);
		}

		public bool IsValidVersion(string version)
		{
			VersionedRequestSettings versionedRequestSetting;
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			return this.TryGetSettingsForVersion(version, out versionedRequestSetting);
		}

		public bool TryGetSettingsForVersion(string version, out VersionedRequestSettings versionSettings)
		{
			if (version == null)
			{
				throw new ArgumentNullException("version");
			}
			version = (false ? version.Trim() : version);
			return this.EnabledVersions.TryGetValue(version, out versionSettings);
		}

		public void UpdateEnabledVersions(List<string> disabledVersionList, string latestEnabledVersion)
		{
			DateTime dateTime;
			if (disabledVersionList == null && string.IsNullOrEmpty(latestEnabledVersion))
			{
				return;
			}
			if (!DateTime.TryParse(latestEnabledVersion, out dateTime) || string.Compare(latestEnabledVersion, "2015-12-11", StringComparison.Ordinal) < 0)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Latest enabled version {0} is not a valid date.", new object[] { latestEnabledVersion });
				latestEnabledVersion = string.Empty;
			}
			Dictionary<string, VersionedRequestSettings> strs = new Dictionary<string, VersionedRequestSettings>();
			foreach (VersionedRequestSettings allVersion in this.AllVersions)
			{
				bool flag = false;
				if (disabledVersionList != null)
				{
					foreach (string str in disabledVersionList)
					{
						if (!str.Equals(allVersion.VersionString, StringComparison.InvariantCulture))
						{
							continue;
						}
						flag = true;
						break;
					}
				}
				if (flag || !string.IsNullOrEmpty(latestEnabledVersion) && string.Compare(allVersion.VersionString, latestEnabledVersion, StringComparison.Ordinal) > 0)
				{
					continue;
				}
				strs.Add(allVersion.VersionString, allVersion);
			}
			this.EnabledVersions = strs;
		}
	}
}