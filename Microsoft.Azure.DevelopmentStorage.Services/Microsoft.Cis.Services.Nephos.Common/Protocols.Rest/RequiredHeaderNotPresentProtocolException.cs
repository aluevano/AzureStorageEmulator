using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class RequiredHeaderNotPresentProtocolException : ProtocolException
	{
		private string headerName;

		public string HeaderName
		{
			get
			{
				return this.headerName;
			}
			set
			{
				this.headerName = value;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				if (string.Compare(this.HeaderName, "Content-Length", StringComparison.Ordinal) == 0)
				{
					return CommonStatusEntries.MissingContentLengthHeader;
				}
				return CommonStatusEntries.MissingRequiredHeader;
			}
		}

		public RequiredHeaderNotPresentProtocolException(string headerName) : this(headerName, (Exception)null)
		{
		}

		public RequiredHeaderNotPresentProtocolException(string headerName, string message) : base(message)
		{
			this.headerName = headerName;
		}

		public RequiredHeaderNotPresentProtocolException(string headerName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The header {0} must be supplied", new object[] { headerName }), innerException)
		{
			this.headerName = headerName;
		}

		public RequiredHeaderNotPresentProtocolException(string headerName, string message, Exception innerException) : base(message, innerException)
		{
			this.headerName = headerName;
		}

		protected RequiredHeaderNotPresentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
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
			return new RequiredHeaderNotPresentProtocolException(this.headerName, this.Message, this);
		}
	}
}