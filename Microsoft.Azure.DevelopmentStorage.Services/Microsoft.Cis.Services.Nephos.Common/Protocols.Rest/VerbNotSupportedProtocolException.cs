using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the REST method which was called must be provided.")]
	public class VerbNotSupportedProtocolException : ProtocolException
	{
		private ReadOnlyCollection<RestMethod> allowedMethods;

		private RestMethod calledMethod;

		public ReadOnlyCollection<RestMethod> AllowedMethods
		{
			get
			{
				return this.allowedMethods;
			}
		}

		public RestMethod CalledMethod
		{
			get
			{
				return this.calledMethod;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.UnsupportedHttpVerb;
			}
		}

		public VerbNotSupportedProtocolException(RestMethod calledMethod, ReadOnlyCollection<RestMethod> allowedMethods) : this(calledMethod, allowedMethods, null)
		{
		}

		public VerbNotSupportedProtocolException(RestMethod calledMethod, ReadOnlyCollection<RestMethod> allowedMethods, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The method {0} is not supported for the specified resource. Refer to 'Allow' response header for the list of supported methods.", new object[] { calledMethod }), innerException)
		{
			NephosAssertionException.Assert(calledMethod != RestMethod.Unknown);
			NephosAssertionException.Assert(allowedMethods != null);
			NephosAssertionException.Assert(allowedMethods.Count > 0);
			this.calledMethod = calledMethod;
			this.allowedMethods = allowedMethods;
		}

		protected VerbNotSupportedProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.calledMethod = (RestMethod)info.GetValue("this.calledMethod", typeof(RestMethod));
			this.allowedMethods = (ReadOnlyCollection<RestMethod>)info.GetValue("this.allowedMethods", typeof(ReadOnlyCollection<RestMethod>));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.calledMethod", this.calledMethod);
			info.AddValue("this.allowedMethods", this.allowedMethods);
			base.GetObjectData(info, context);
		}

		public override NameValueCollection GetResponseHeaders()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(1);
			foreach (RestMethod allowedMethod in this.AllowedMethods)
			{
				nameValueCollection.Add("Allow", allowedMethod.ToString());
			}
			return nameValueCollection;
		}

		public override Exception GetRethrowableClone()
		{
			return new VerbNotSupportedProtocolException(this.calledMethod, this.allowedMethods, this);
		}
	}
}