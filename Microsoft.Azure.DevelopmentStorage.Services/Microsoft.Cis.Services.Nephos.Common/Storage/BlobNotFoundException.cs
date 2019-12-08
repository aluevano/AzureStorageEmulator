using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobNotFoundException : StorageManagerException
	{
		public BlobNotFoundException() : base("The blob does not exist.")
		{
		}

		public BlobNotFoundException(string message) : base(message)
		{
		}

		public BlobNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobNotFoundException(this.Message, this);
		}
	}
}