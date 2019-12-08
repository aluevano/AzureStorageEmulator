using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobAlreadyExistsException : StorageManagerException
	{
		public BlobAlreadyExistsException() : base("The blob already exists.")
		{
		}

		public BlobAlreadyExistsException(string message) : base(message)
		{
		}

		public BlobAlreadyExistsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobAlreadyExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobAlreadyExistsException(this.Message, this);
		}
	}
}