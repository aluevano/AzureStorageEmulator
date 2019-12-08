using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidSourceBlobUrlException : StorageManagerException
	{
		public InvalidSourceBlobUrlException() : base("Copy source in incremental copy must be a valid blob url.")
		{
		}

		public InvalidSourceBlobUrlException(string message) : base(message)
		{
		}

		public InvalidSourceBlobUrlException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidSourceBlobUrlException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidSourceBlobUrlException(this.Message, this);
		}
	}
}