using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobGenerationIdNotFoundException : StorageManagerException
	{
		public BlobGenerationIdNotFoundException() : base("Differential get page ranges is not supported on the blob.")
		{
		}

		public BlobGenerationIdNotFoundException(string message) : base(message)
		{
		}

		public BlobGenerationIdNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobGenerationIdNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobGenerationIdNotFoundException(this.Message, this);
		}
	}
}