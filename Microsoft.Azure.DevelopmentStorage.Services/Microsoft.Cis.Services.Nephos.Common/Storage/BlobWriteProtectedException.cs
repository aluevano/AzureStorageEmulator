using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlobWriteProtectedException : StorageManagerException
	{
		public BlobWriteProtectedException() : base("The specified blob is write protected.")
		{
		}

		public BlobWriteProtectedException(string message) : base(message)
		{
		}

		public BlobWriteProtectedException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlobWriteProtectedException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlobWriteProtectedException(this.Message, this);
		}
	}
}