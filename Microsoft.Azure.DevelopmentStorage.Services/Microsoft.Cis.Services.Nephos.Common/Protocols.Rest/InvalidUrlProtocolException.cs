using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the invalid URL path must be provided.")]
	public class InvalidUrlProtocolException : ProtocolException
	{
		private string uriPath;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidUri;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification="This represents an invalid URL, therefore should not be represented by a Uri object which will throw if given an invalid URL!  In particular, this is an exception class, so we don't want any exceptions being thrown when creating it.")]
		public string UriPath
		{
			get
			{
				return this.uriPath;
			}
		}

		[SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification="The paths will actually be invalid URLs!")]
		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification="The paths will actually be invalid URLs!")]
		public InvalidUrlProtocolException(string urlPath) : this(urlPath, null)
		{
		}

		public InvalidUrlProtocolException(Uri urlPath) : this(urlPath, null)
		{
		}

		[SuppressMessage("Microsoft.Design", "CA1057:StringUriOverloadsCallSystemUriOverloads", Justification="The paths will actually be invalid URLs!")]
		public InvalidUrlProtocolException(string urlPath, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "URL path {0} doesn't represent any valid resource on the server", new object[] { HttpUtilities.GetSafeUriString(urlPath) }), innerException)
		{
			if (urlPath == null)
			{
				throw new ArgumentNullException("urlPath");
			}
			this.uriPath = urlPath;
		}

		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification="The paths will actually be invalid URLs!")]
		public InvalidUrlProtocolException(Uri urlPath, Exception innerException) : this((urlPath != null ? urlPath.ToString() : string.Empty), innerException)
		{
		}

		protected InvalidUrlProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.uriPath = info.GetString("this.uriPath");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "UriPath", this.UriPath }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.uriPath", this.uriPath);
			base.GetObjectData(info, context);
		}

		[SuppressMessage("Microsoft.Usage", "CA2234:PassSystemUriObjectsInsteadOfStrings", Justification="The paths will actually be invalid URLs!")]
		public override Exception GetRethrowableClone()
		{
			return new InvalidUrlProtocolException(this.UriPath, this);
		}
	}
}