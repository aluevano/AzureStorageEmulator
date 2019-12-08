using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ObjectMetadataOverLimitException : StorageManagerException
	{
		public ObjectMetadataOverLimitException() : base("The object metadata size is over the limit.")
		{
		}

		public ObjectMetadataOverLimitException(string message) : base(message)
		{
		}

		public ObjectMetadataOverLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ObjectMetadataOverLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ObjectMetadataOverLimitException(this.Message, this);
		}
	}
}