using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class UncommittedBlockCountExceedsLimitException : StorageManagerException
	{
		public UncommittedBlockCountExceedsLimitException() : base("The uncommitted block count exceeds the maximum limit of 100,000 blocks.")
		{
		}

		public UncommittedBlockCountExceedsLimitException(string message) : base(message)
		{
		}

		public UncommittedBlockCountExceedsLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UncommittedBlockCountExceedsLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new UncommittedBlockCountExceedsLimitException(this.Message, this);
		}
	}
}