using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class MaxBlobSizeConditionNotMetException : StorageManagerException
	{
		public MaxBlobSizeConditionNotMetException() : base("The max blob size condition specified was not met.")
		{
		}

		public MaxBlobSizeConditionNotMetException(string message) : base(message)
		{
		}

		public MaxBlobSizeConditionNotMetException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected MaxBlobSizeConditionNotMetException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new MaxBlobSizeConditionNotMetException(this.Message, this);
		}
	}
}