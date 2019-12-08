using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class FeatureNotSupportedByEmulatorException : ProtocolException
	{
		private string featureUsed;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return EmulatorStatusEntries.FeatureNotSupportedByEmulator;
			}
		}

		public FeatureNotSupportedByEmulatorException(string featureUsed) : this(featureUsed, null)
		{
		}

		public FeatureNotSupportedByEmulatorException(string featureUsed, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The {0} feature is not currently supported by the emulator.", new object[] { featureUsed }), innerException)
		{
			this.featureUsed = featureUsed;
		}

		protected FeatureNotSupportedByEmulatorException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.featureUsed = (string)info.GetValue("featureUsed", typeof(string));
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection()
			{
				{ "Feature", this.featureUsed }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("featureUsed", this.featureUsed);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new FeatureNotSupportedByEmulatorException(this.featureUsed, this);
		}
	}
}