using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InvalidSourceBlobTypeException : StorageManagerException
	{
		public InvalidSourceBlobTypeException() : base("Copy source in incremental copy must be a valid blob type (page blob).")
		{
		}

		public InvalidSourceBlobTypeException(string message) : base(message)
		{
		}

		public InvalidSourceBlobTypeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InvalidSourceBlobTypeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidSourceBlobTypeException(this.Message, this);
		}
	}
}