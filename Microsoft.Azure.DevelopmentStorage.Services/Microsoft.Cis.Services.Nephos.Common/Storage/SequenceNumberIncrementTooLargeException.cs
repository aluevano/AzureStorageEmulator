using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class SequenceNumberIncrementTooLargeException : StorageManagerException
	{
		public SequenceNumberIncrementTooLargeException() : base("The sequence number increment cannot be performed because it would result in overflow of the sequence number.")
		{
		}

		public SequenceNumberIncrementTooLargeException(string message) : base(message)
		{
		}

		public SequenceNumberIncrementTooLargeException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SequenceNumberIncrementTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new SequenceNumberIncrementTooLargeException(this.Message, this);
		}
	}
}