using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class PendingAccountTransactionExistsException : StorageManagerException
	{
		public PendingAccountTransactionExistsException() : base("The account has a pending transaction which must finish before a new one can start.")
		{
		}

		public PendingAccountTransactionExistsException(string message) : base(message)
		{
		}

		public PendingAccountTransactionExistsException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected PendingAccountTransactionExistsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new PendingAccountTransactionExistsException(this.Message, this);
		}
	}
}