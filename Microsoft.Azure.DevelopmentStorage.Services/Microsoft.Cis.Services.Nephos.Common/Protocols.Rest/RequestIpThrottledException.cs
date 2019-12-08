using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	internal class RequestIpThrottledException : Exception, IRethrowableException
	{
		public RequestIpThrottledException(string message) : base(message)
		{
		}

		public RequestIpThrottledException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RequestIpThrottledException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new RequestIpThrottledException(this.Message, base.InnerException);
		}
	}
}