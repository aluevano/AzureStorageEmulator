using AsyncHelper;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class CorsPreflightFailureException : Exception, IRethrowableException
	{
		public CorsPreflightFailureException() : base("CORS preflight request failure")
		{
		}

		public CorsPreflightFailureException(string message) : base(message)
		{
		}

		public CorsPreflightFailureException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new CorsPreflightFailureException(this.Message, this);
		}
	}
}