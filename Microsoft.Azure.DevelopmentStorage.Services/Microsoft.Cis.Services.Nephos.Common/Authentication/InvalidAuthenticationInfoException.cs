using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Serializable]
	public class InvalidAuthenticationInfoException : Exception, IRethrowableException
	{
		public InvalidAuthenticationInfoException()
		{
		}

		public InvalidAuthenticationInfoException(string message) : base(message)
		{
		}

		public InvalidAuthenticationInfoException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidAuthenticationInfoException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new InvalidAuthenticationInfoException(base.Message, this);
		}
	}
}