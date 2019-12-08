using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class FutureVersionProtocolException : ProtocolException
	{
		private string versionUsed;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return EmulatorStatusEntries.VersionNotSupportedByEmulator;
			}
		}

		public FutureVersionProtocolException(string versionUsed) : this(versionUsed, null)
		{
		}

		public FutureVersionProtocolException(string versionUsed, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The REST version {0} is not supported is this version of the emulator.", new object[] { versionUsed }), innerException)
		{
			this.versionUsed = versionUsed;
		}

		protected FutureVersionProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.versionUsed = (string)info.GetValue("versionUsed", typeof(string));
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection()
			{
				{ "VersionAttempted", this.versionUsed }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("versionUsed", this.versionUsed);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new FutureVersionProtocolException(this.versionUsed, this);
		}

		public static void ThrowIfFutureVersion(string version)
		{
			if (version == null)
			{
				return;
			}
			if (!VersioningConfigurationLookup.Instance.IsValidVersion(version) && string.CompareOrdinal(VersioningConfigurationLookup.Instance.LatestVersion, version) < 0)
			{
				throw new FutureVersionProtocolException(version);
			}
		}
	}
}