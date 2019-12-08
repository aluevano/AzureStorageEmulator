using AsyncHelper;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="This exception provides a wrapper around the original exception so that it is rethrowable. Hence, only construcor that takes the message and the actual exception is provided, since this is the only way it must be used.")]
	public class RethrowableTableServiceException : Exception, IRethrowableException
	{
		public RethrowableTableServiceException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected RethrowableTableServiceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new RethrowableTableServiceException(this.Message, base.InnerException);
		}
	}
}