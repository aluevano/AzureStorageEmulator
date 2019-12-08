using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobMD5MigrationException : StorageManagerException
	{
		public BlobMD5MigrationException() : base("The blob MD5 migration failed.")
		{
		}

		public BlobMD5MigrationException(string message) : base(message)
		{
		}

		public BlobMD5MigrationException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobMD5MigrationException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobMD5MigrationException(this.Message, this);
		}
	}
}