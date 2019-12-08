using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class ObjectServiceMetadataOverLimitException : StorageManagerException
	{
		public ObjectServiceMetadataOverLimitException() : base("The object service metadata is over the limit.")
		{
		}

		public ObjectServiceMetadataOverLimitException(string message) : base(message)
		{
		}

		public ObjectServiceMetadataOverLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected ObjectServiceMetadataOverLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new ObjectServiceMetadataOverLimitException(this.Message, this);
		}
	}
}