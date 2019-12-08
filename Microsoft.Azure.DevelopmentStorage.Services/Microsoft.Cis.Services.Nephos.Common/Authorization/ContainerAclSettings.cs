using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Collections.Generic;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public class ContainerAclSettings : BaseAclSettings
	{
		public override bool PublicAccess
		{
			get
			{
				if (string.IsNullOrEmpty(base.PublicAccessLevel))
				{
					return false;
				}
				if (Comparison.StringEqualsIgnoreCase(base.PublicAccessLevel, bool.TrueString))
				{
					return true;
				}
				return Comparison.StringEqualsIgnoreCase(base.PublicAccessLevel, "container");
			}
		}

		public ContainerAclSettings()
		{
		}

		public ContainerAclSettings(bool? publicAccess) : this((publicAccess.HasValue ? publicAccess.ToString() : null))
		{
		}

		public ContainerAclSettings(bool? publicAccess, string version) : this((publicAccess.HasValue ? publicAccess.ToString() : null), version)
		{
		}

		public ContainerAclSettings(string publicAccessLevel) : this(publicAccessLevel, new List<SASIdentifier>())
		{
		}

		public ContainerAclSettings(string publicAccessLevel, string version) : this(publicAccessLevel, new List<SASIdentifier>(), version)
		{
		}

		public ContainerAclSettings(bool publicAccess, List<SASIdentifier> sasIdentifiers) : this((publicAccess ? "container" : null), sasIdentifiers)
		{
		}

		public ContainerAclSettings(bool publicAccess, List<SASIdentifier> sasIdentifiers, string version) : this((publicAccess ? "container" : null), sasIdentifiers, version)
		{
		}

		public ContainerAclSettings(string publicAccessLevel, List<SASIdentifier> sasIdentifiers) : this(publicAccessLevel, sasIdentifiers, null)
		{
		}

		public ContainerAclSettings(string publicAccessLevel, List<SASIdentifier> sasIdentifiers, string version) : base(publicAccessLevel, sasIdentifiers, version)
		{
		}

		public ContainerAclSettings(byte[] serviceMetadata) : base(serviceMetadata)
		{
		}
	}
}