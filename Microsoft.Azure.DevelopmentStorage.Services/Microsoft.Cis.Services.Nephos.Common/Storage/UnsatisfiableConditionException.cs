using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class UnsatisfiableConditionException : StorageManagerException
	{
		public UnsatisfiableConditionException() : base("The request includes an unsatisfiable condition for this operation.")
		{
		}

		public UnsatisfiableConditionException(string message) : base(message)
		{
		}

		public UnsatisfiableConditionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected UnsatisfiableConditionException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new UnsatisfiableConditionException(this.Message, this);
		}
	}
}