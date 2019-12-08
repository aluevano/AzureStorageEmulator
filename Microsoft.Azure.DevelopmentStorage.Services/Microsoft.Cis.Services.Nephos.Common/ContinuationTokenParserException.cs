using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class ContinuationTokenParserException : Exception, IRethrowableException
	{
		public ContinuationTokenParserException()
		{
		}

		public ContinuationTokenParserException(string message) : base(message)
		{
		}

		public ContinuationTokenParserException(string message, string param) : base(string.Format("Invalid param '{0}'. Message: {1}", param, message))
		{
		}

		public ContinuationTokenParserException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ContinuationTokenParserException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public virtual Exception GetRethrowableClone()
		{
			return new ContinuationTokenParserException(base.Message, this);
		}
	}
}