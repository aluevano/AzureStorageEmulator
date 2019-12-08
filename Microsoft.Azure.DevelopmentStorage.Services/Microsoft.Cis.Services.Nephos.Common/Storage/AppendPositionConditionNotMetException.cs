using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class AppendPositionConditionNotMetException : StorageManagerException
	{
		public AppendPositionConditionNotMetException() : base("The append position condition was not met.")
		{
		}

		public AppendPositionConditionNotMetException(string message) : base(message)
		{
		}

		public AppendPositionConditionNotMetException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AppendPositionConditionNotMetException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new AppendPositionConditionNotMetException(this.Message, this);
		}
	}
}