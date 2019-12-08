using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class ForbiddenRequestAbortedException : Exception, IRethrowableException
	{
		public ForbiddenRequestAbortedException(string message) : base(message)
		{
		}

		public ForbiddenRequestAbortedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ForbiddenRequestAbortedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new ForbiddenRequestAbortedException(this.Message, base.InnerException);
		}
	}
}