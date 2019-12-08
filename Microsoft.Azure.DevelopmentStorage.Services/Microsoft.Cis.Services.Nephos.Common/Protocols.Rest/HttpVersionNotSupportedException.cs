using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the version must be provided.")]
	public class HttpVersionNotSupportedException : ProtocolException
	{
		private Version version;

		private string via;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.UnsupportedHttpVersion;
			}
		}

		public HttpVersionNotSupportedException(Version version, string via) : this(version, via, null)
		{
		}

		public HttpVersionNotSupportedException(Version version, string via, Exception innerException)
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] objArray = new object[] { version, null };
			object[] objArray1 = objArray;
			if (string.IsNullOrEmpty(via))
			{
				object obj = " empty.";
			}
			else
			{
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] objArray2 = new object[] { via };
				obj = string.Format(cultureInfo, ": '{0}'.", objArray2);
			}
			objArray1[1] = obj;
			base(string.Format(invariantCulture, "The HTTP version {0} specified in the request is not supported for this operation by the server. The value of Header Via is{1}", objArray), innerException);
			this.version = version;
			this.via = via;
		}

		protected HttpVersionNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.version = (Version)info.GetValue("this.version", typeof(Version));
			this.via = info.GetString("this.via");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(2)
			{
				{ "Version", this.version.ToString() },
				{ "Via", this.via }
			};
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.version", this.version);
			info.AddValue("this.via", this.via);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new HttpVersionNotSupportedException(this.version, this.via, this);
		}
	}
}