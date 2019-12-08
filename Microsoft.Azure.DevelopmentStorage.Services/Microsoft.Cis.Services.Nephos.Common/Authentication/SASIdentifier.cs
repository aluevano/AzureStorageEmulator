using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class SASIdentifier
	{
		public readonly static string IdName;

		public readonly static int MaxIdLength;

		public readonly static int MaxSASIdentifiers;

		public SASAccessPolicy AccessPolicy
		{
			get;
			set;
		}

		public string Id
		{
			get;
			set;
		}

		static SASIdentifier()
		{
			SASIdentifier.IdName = "id";
			SASIdentifier.MaxIdLength = 64;
			SASIdentifier.MaxSASIdentifiers = 5;
		}

		public SASIdentifier()
		{
			this.AccessPolicy = new SASAccessPolicy();
		}

		public SASIdentifier(string id, DateTime signedStart, DateTime signedExpiry, SASPermission signedPermission)
		{
			this.Id = id;
			this.AccessPolicy = new SASAccessPolicy(signedStart, signedExpiry, signedPermission);
		}

		public SASIdentifier(string id, string signedStart, string signedExpiry, string signedPermission)
		{
			this.Id = id;
			this.AccessPolicy = new SASAccessPolicy(signedStart, signedExpiry, signedPermission);
		}

		public void Decode(string sasIdentifier)
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			SASUtilities.ValidateACIIEncoding(sasIdentifier);
			NameValueCollection nameValueCollection = new NameValueCollection();
			MetadataEncoding.Decode(aSCIIEncoding.GetBytes(sasIdentifier), nameValueCollection);
			string item = nameValueCollection[SASIdentifier.IdName];
			string str = nameValueCollection[SASAccessPolicy.SignedStartName];
			string item1 = nameValueCollection[SASAccessPolicy.SignedExpiryName];
			string str1 = nameValueCollection[SASAccessPolicy.SignedPermissionName];
			if (!string.IsNullOrEmpty(item))
			{
				this.Id = unicodeEncoding.GetString(Convert.FromBase64String(item));
			}
			if (!string.IsNullOrEmpty(str))
			{
				this.AccessPolicy.SignedStart = new DateTime?(SASUtilities.ParseTime(str));
			}
			if (!string.IsNullOrEmpty(item1))
			{
				this.AccessPolicy.SignedExpiry = new DateTime?(SASUtilities.ParseTime(item1));
			}
			if (!string.IsNullOrEmpty(str1))
			{
				this.AccessPolicy.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(str1));
			}
		}

		public byte[] Encode()
		{
			ASCIIEncoding aSCIIEncoding = new ASCIIEncoding();
			UnicodeEncoding unicodeEncoding = new UnicodeEncoding();
			NameValueCollection nameValueCollection = new NameValueCollection();
			nameValueCollection[SASIdentifier.IdName] = Convert.ToBase64String(unicodeEncoding.GetBytes(this.Id));
			if (this.AccessPolicy.SignedStart.HasValue)
			{
				string signedStartName = SASAccessPolicy.SignedStartName;
				DateTime? signedStart = this.AccessPolicy.SignedStart;
				nameValueCollection[signedStartName] = SASUtilities.EncodeTime(signedStart.Value);
			}
			if (this.AccessPolicy.SignedExpiry.HasValue)
			{
				string signedExpiryName = SASAccessPolicy.SignedExpiryName;
				DateTime? signedExpiry = this.AccessPolicy.SignedExpiry;
				nameValueCollection[signedExpiryName] = SASUtilities.EncodeTime(signedExpiry.Value);
			}
			if (this.AccessPolicy.SignedPermission.HasValue)
			{
				string signedPermissionName = SASAccessPolicy.SignedPermissionName;
				SASPermission? signedPermission = this.AccessPolicy.SignedPermission;
				nameValueCollection[signedPermissionName] = SASUtilities.EncodeSASPermission(signedPermission.Value);
			}
			return MetadataEncoding.Encode(nameValueCollection);
		}

		public override bool Equals(object obj)
		{
			return this == obj as SASIdentifier;
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}

		public static bool operator ==(SASIdentifier l, SASIdentifier r)
		{
			if (l.Id != r.Id)
			{
				return false;
			}
			return l.AccessPolicy == r.AccessPolicy;
		}

		public static bool operator !=(SASIdentifier l, SASIdentifier r)
		{
			if (l.Id != r.Id)
			{
				return true;
			}
			return l.AccessPolicy != r.AccessPolicy;
		}
	}
}