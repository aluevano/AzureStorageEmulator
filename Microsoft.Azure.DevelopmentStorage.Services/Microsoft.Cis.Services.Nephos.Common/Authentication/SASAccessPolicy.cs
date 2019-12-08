using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	public class SASAccessPolicy
	{
		public readonly static string SignedStartName;

		public readonly static string SignedExpiryName;

		public readonly static string SignedPermissionName;

		public DateTime? SignedExpiry
		{
			get;
			set;
		}

		public SASPermission? SignedPermission
		{
			get;
			set;
		}

		public DateTime? SignedStart
		{
			get;
			set;
		}

		static SASAccessPolicy()
		{
			SASAccessPolicy.SignedStartName = "st";
			SASAccessPolicy.SignedExpiryName = "se";
			SASAccessPolicy.SignedPermissionName = "sp";
		}

		public SASAccessPolicy()
		{
		}

		public SASAccessPolicy(DateTime signedStart, DateTime signedExpiry, SASPermission signedPermission)
		{
			this.SignedStart = new DateTime?(signedStart);
			this.SignedExpiry = new DateTime?(signedExpiry);
			this.SignedPermission = new SASPermission?(signedPermission);
		}

		public SASAccessPolicy(string signedStart, string signedExpiry, string signedPermission)
		{
			if (!string.IsNullOrEmpty(signedStart))
			{
				this.SignedStart = new DateTime?(SASUtilities.ParseTime(signedStart));
			}
			if (!string.IsNullOrEmpty(signedExpiry))
			{
				this.SignedExpiry = new DateTime?(SASUtilities.ParseTime(signedExpiry));
			}
			if (!string.IsNullOrEmpty(signedPermission))
			{
				this.SignedPermission = new SASPermission?(SASUtilities.ParseSASPermission(signedPermission));
			}
		}

		public override bool Equals(object obj)
		{
			return this == obj as SASAccessPolicy;
		}

		public override int GetHashCode()
		{
			return this.GetHashCode();
		}

		public static bool operator ==(SASAccessPolicy l, SASAccessPolicy r)
		{
			return l == r;
		}

		public static bool operator !=(SASAccessPolicy l, SASAccessPolicy r)
		{
			return l != r;
		}
	}
}