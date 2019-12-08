using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class SequenceNumberConditionNotMetException : StorageManagerException
	{
		public SequenceNumberConditionNotMetException() : base("The sequence number condition was not met.")
		{
		}

		public SequenceNumberConditionNotMetException(string message) : base(message)
		{
		}

		public SequenceNumberConditionNotMetException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected SequenceNumberConditionNotMetException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new SequenceNumberConditionNotMetException(this.Message, this);
		}
	}
}