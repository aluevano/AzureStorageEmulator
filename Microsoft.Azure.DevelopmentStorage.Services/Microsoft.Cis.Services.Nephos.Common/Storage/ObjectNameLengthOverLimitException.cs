using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ObjectNameLengthOverLimitException : StorageManagerException
	{
		public ObjectNameLengthOverLimitException() : base("The object name length is over the limit.")
		{
		}

		public ObjectNameLengthOverLimitException(string message) : base(message)
		{
		}

		public ObjectNameLengthOverLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ObjectNameLengthOverLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ObjectNameLengthOverLimitException(this.Message, this);
		}
	}
}