using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the unknown verb must be provided.")]
	public class UnknownVerbProtocolException : ProtocolException
	{
		private string verb;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidHttpVerb;
			}
		}

		public string Verb
		{
			get
			{
				return this.verb;
			}
		}

		public UnknownVerbProtocolException(string verb) : this(verb, null)
		{
		}

		public UnknownVerbProtocolException(string verb, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The HTTP verb {0} specified in the request is not recognized by the server", new object[] { verb }), innerException)
		{
			this.verb = verb;
		}

		protected UnknownVerbProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.verb = info.GetString("this.verb");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "Verb", this.Verb }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.verb", this.verb);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new UnknownVerbProtocolException(this.Verb, this);
		}
	}
}