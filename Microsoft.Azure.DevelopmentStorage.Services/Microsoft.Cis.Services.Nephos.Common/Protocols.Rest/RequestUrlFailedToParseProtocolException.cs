using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the raw url that failed to parse must be provided.")]
	public class RequestUrlFailedToParseProtocolException : ProtocolException
	{
		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.RequestUrlFailedToParse;
			}
		}

		public RequestUrlFailedToParseProtocolException(string message) : base(message)
		{
		}

		public RequestUrlFailedToParseProtocolException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RequestUrlFailedToParseProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new RequestUrlFailedToParseProtocolException(this.Message, this);
		}
	}
}