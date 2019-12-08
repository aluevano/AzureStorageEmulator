using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Authorization
{
	public abstract class BaseAclSettings
	{
		public const string PublicAccessMetadataName = "PublicAccess";

		public const string PublicAccess1MetadataName = "PublicAccess1";

		public const string SASIdentifiersName = "SASIdentifiers";

		private string publicAccessLevel;

		private string version;

		private List<SASIdentifier> sasIdentifiers = new List<SASIdentifier>();

		public abstract bool PublicAccess
		{
			get;
		}

		public string PublicAccessLevel
		{
			get
			{
				return this.publicAccessLevel;
			}
			set
			{
				this.publicAccessLevel = value;
			}
		}

		public List<SASIdentifier> SASIdentifiers
		{
			get
			{
				return this.sasIdentifiers;
			}
			set
			{
				this.sasIdentifiers = value;
			}
		}

		public string Version
		{
			get
			{
				return this.version;
			}
		}

		public BaseAclSettings()
		{
			this.sasIdentifiers = new List<SASIdentifier>();
		}

		protected BaseAclSettings(bool? publicAccess) : this((publicAccess.HasValue ? publicAccess.ToString() : null))
		{
		}

		protected BaseAclSettings(bool? publicAccess, string version) : this((publicAccess.HasValue ? publicAccess.ToString() : null), version)
		{
		}

		protected BaseAclSettings(string publicAccessLevel) : this(publicAccessLevel, new List<SASIdentifier>())
		{
		}

		protected BaseAclSettings(string publicAccessLevel, string version) : this(publicAccessLevel, new List<SASIdentifier>(), version)
		{
		}

		protected BaseAclSettings(bool publicAccess, List<SASIdentifier> sasIdentifiers) : this((publicAccess ? "container" : null), sasIdentifiers)
		{
		}

		protected BaseAclSettings(bool publicAccess, List<SASIdentifier> sasIdentifiers, string version) : this((publicAccess ? "container" : null), sasIdentifiers, version)
		{
		}

		protected BaseAclSettings(string publicAccessLevel, List<SASIdentifier> sasIdentifiers) : this(publicAccessLevel, sasIdentifiers, null)
		{
		}

		protected BaseAclSettings(string publicAccessLevel, List<SASIdentifier> sasIdentifiers, string version)
		{
			this.publicAccessLevel = publicAccessLevel;
			if (sasIdentifiers == null)
			{
				throw new ArgumentException("sasIdentifiers");
			}
			this.sasIdentifiers = sasIdentifiers;
			this.version = version;
		}

		protected BaseAclSettings(byte[] serviceMetadata)
		{
			if (serviceMetadata != null)
			{
				NameValueCollection nameValueCollection = new NameValueCollection();
				MetadataEncoding.Decode(serviceMetadata, nameValueCollection);
				string str = nameValueCollection.Get("PublicAccess");
				string str1 = nameValueCollection.Get("PublicAccess1");
				string str2 = nameValueCollection.Get("SASIdentifiers");
				if (str != null)
				{
					this.publicAccessLevel = str;
				}
				else if (str1 != null)
				{
					this.publicAccessLevel = str1;
				}
				if (!string.IsNullOrEmpty(str2))
				{
					this.sasIdentifiers = SASUtilities.DecodeSASIdentifiers(str2);
				}
			}
		}

		public void EncodeToServiceMetadata(out byte[] serviceMetadata)
		{
			NameValueCollection nameValueCollection;
			this.ToNameValues(out nameValueCollection);
			serviceMetadata = MetadataEncoding.Encode(nameValueCollection);
		}

		public void ToNameValues(out NameValueCollection nameValues)
		{
			nameValues = new NameValueCollection();
			if (VersioningHelper.IsPreSeptember09OrInvalidVersion(this.version))
			{
				bool publicAccess = this.PublicAccess;
				nameValues.Add("PublicAccess", publicAccess.ToString(CultureInfo.InvariantCulture));
			}
			else if (!string.IsNullOrEmpty(this.publicAccessLevel))
			{
				nameValues.Add("PublicAccess1", this.publicAccessLevel.ToString(CultureInfo.InvariantCulture));
			}
			nameValues.Add("SASIdentifiers", SASUtilities.EncodeSASIdentifiers(this.sasIdentifiers));
		}
	}
}