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
	public class InvalidHeaderProtocolException : ProtocolException
	{
		private string headerName;

		private string headerValue;

		public string HeaderName
		{
			get
			{
				return this.headerName;
			}
		}

		public string HeaderValue
		{
			get
			{
				return this.headerValue;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidHeaderValue;
			}
		}

		public InvalidHeaderProtocolException(string headerName, string headerValue) : this(headerName, headerValue, null)
		{
		}

		public InvalidHeaderProtocolException(string headerName, string headerValue, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The value {0} provided for request header {1} is invalid.", new object[] { headerValue, headerName }), innerException)
		{
			this.headerName = headerName;
			this.headerValue = headerValue;
		}

		protected InvalidHeaderProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.headerName = info.GetString("this.headerName");
			this.headerValue = info.GetString("this.headerValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(2)
			{
				{ "HeaderName", this.HeaderName },
				{ "HeaderValue", this.HeaderValue }
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
			info.AddValue("this.headerName", this.headerName);
			info.AddValue("this.headerValue", this.headerValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidHeaderProtocolException(this.HeaderName, this.HeaderValue, this);
		}
	}
}