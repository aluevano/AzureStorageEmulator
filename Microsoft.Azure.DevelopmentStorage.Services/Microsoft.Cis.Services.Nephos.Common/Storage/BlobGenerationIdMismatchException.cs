using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobGenerationIdMismatchException : StorageManagerException
	{
		public BlobGenerationIdMismatchException() : base("Blobs are from different generations.")
		{
		}

		public BlobGenerationIdMismatchException(string message) : base(message)
		{
		}

		public BlobGenerationIdMismatchException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobGenerationIdMismatchException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobGenerationIdMismatchException(this.Message, this);
		}
	}
}