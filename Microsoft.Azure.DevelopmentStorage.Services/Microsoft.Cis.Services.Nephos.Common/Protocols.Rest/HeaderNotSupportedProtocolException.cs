using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the header name and value must be provided.")]
	public class HeaderNotSupportedProtocolException : ProtocolException
	{
		private string headerName;

		public string HeaderName
		{
			get
			{
				return this.headerName;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.UnsupportedHeader;
			}
		}

		public HeaderNotSupportedProtocolException(string headerName) : this(headerName, null)
		{
		}

		public HeaderNotSupportedProtocolException(string headerName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The header '{0}' is not supported for the given operation.", new object[] { headerName }), innerException)
		{
			this.headerName = headerName;
		}

		protected HeaderNotSupportedProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.headerName = info.GetString("this.headerName");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "HeaderName", this.HeaderName }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.headerName", this.headerName);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new HeaderNotSupportedProtocolException(this.HeaderName, this);
		}
	}
}