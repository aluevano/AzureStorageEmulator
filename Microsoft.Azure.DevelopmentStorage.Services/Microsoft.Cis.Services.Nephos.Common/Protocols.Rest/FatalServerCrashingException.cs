using AsyncHelper;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="This exception provides a wrapper around the original exception so that it is rethrowable. Hence, only constructor that takes the message and the actual exception is provided, since this is the only way it must be used.")]
	internal class FatalServerCrashingException : Exception, IRethrowableException
	{
		public FatalServerCrashingException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected FatalServerCrashingException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new FatalServerCrashingException(this.Message, base.InnerException);
		}
	}
}