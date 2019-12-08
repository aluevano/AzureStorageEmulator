using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Serializable]
	public class AuthenticationFailureException : Exception, IRethrowableException
	{
		public AuthenticationFailureException()
		{
		}

		public AuthenticationFailureException(string message) : base(message)
		{
		}

		public AuthenticationFailureException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AuthenticationFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new AuthenticationFailureException(base.Message, this);
		}
	}
}