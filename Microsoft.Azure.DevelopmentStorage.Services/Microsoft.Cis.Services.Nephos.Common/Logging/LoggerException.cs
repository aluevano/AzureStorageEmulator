using AsyncHelper;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors")]
	public class LoggerException : Exception, IRethrowableException
	{
		public LoggerException(string message) : base(message)
		{
		}

		public LoggerException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LoggerException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new LoggerException(this.Message, this);
		}
	}
}