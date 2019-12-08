using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class NoPendingCopyException : StorageManagerException
	{
		public NoPendingCopyException() : base("There is no pending copy operation.")
		{
		}

		public NoPendingCopyException(string message) : base(message)
		{
		}

		public NoPendingCopyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected NoPendingCopyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new NoPendingCopyException(this.Message, this);
		}
	}
}