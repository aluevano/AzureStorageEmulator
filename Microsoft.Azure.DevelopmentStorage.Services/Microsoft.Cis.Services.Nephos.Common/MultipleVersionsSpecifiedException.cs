using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common
{
	[Serializable]
	public class MultipleVersionsSpecifiedException : Exception, IRethrowableException
	{
		public MultipleVersionsSpecifiedException() : base("Request can specify either x-ms-version header or api-version query parameter but not both.")
		{
		}

		public MultipleVersionsSpecifiedException(string message) : base(message)
		{
		}

		public MultipleVersionsSpecifiedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MultipleVersionsSpecifiedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new MultipleVersionsSpecifiedException(this.Message, base.InnerException);
		}
	}
}