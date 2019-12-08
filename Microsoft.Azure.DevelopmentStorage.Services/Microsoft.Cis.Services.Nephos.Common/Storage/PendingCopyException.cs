using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class PendingCopyException : StorageManagerException
	{
		public PendingCopyException() : base("There is currently a pending copy operation.")
		{
		}

		public PendingCopyException(string message) : base(message)
		{
		}

		public PendingCopyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PendingCopyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new PendingCopyException(this.Message, this);
		}
	}
}