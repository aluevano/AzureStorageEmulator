using AsyncHelper;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class CorsPreflightMissingHeaderException : Exception, IRethrowableException
	{
		public CorsPreflightMissingHeaderException() : base("CORS preflight request failed due to missing required request headers")
		{
		}

		public CorsPreflightMissingHeaderException(string headerName) : base(string.Format("Missing required CORS header {0}", headerName))
		{
		}

		public CorsPreflightMissingHeaderException(string message, Exception innerException) : base(message, innerException)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new CorsPreflightMissingHeaderException(this.Message, this);
		}
	}
}