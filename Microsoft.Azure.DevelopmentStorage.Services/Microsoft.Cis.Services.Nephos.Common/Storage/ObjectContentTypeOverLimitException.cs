using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ObjectContentTypeOverLimitException : StorageManagerException
	{
		public ObjectContentTypeOverLimitException() : base("The object content type size is over the limit.")
		{
		}

		public ObjectContentTypeOverLimitException(string message) : base(message)
		{
		}

		public ObjectContentTypeOverLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ObjectContentTypeOverLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ObjectContentTypeOverLimitException(this.Message, this);
		}
	}
}