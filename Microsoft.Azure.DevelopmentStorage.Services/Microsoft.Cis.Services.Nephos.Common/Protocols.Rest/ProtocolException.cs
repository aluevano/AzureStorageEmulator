using AsyncHelper;
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public abstract class ProtocolException : Exception, IRethrowableException
	{
		public abstract NephosStatusEntry StatusEntry
		{
			get;
		}

		protected ProtocolException()
		{
		}

		protected ProtocolException(string message) : base(message)
		{
		}

		protected ProtocolException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual NameValueCollection GetAdditionalUserDetails()
		{
			return null;
		}

		public virtual NameValueCollection GetResponseHeaders()
		{
			return null;
		}

		public abstract Exception GetRethrowableClone();
	}
}