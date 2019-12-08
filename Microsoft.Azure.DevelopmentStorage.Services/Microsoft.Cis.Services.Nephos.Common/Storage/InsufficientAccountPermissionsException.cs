using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class InsufficientAccountPermissionsException : StorageManagerException
	{
		public InsufficientAccountPermissionsException() : base("The account does not have sufficient permissions to execute the operation.")
		{
		}

		public InsufficientAccountPermissionsException(string message) : base(message)
		{
		}

		public InsufficientAccountPermissionsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected InsufficientAccountPermissionsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new InsufficientAccountPermissionsException(this.Message, this);
		}
	}
}