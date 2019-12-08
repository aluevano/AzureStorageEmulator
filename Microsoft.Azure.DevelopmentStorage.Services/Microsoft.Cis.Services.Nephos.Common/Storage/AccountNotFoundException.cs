using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class AccountNotFoundException : StorageManagerException
	{
		public AccountNotFoundException() : base("The account does not exist.")
		{
		}

		public AccountNotFoundException(string message) : base(message)
		{
		}

		public AccountNotFoundException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected AccountNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new AccountNotFoundException(this.Message, this);
		}
	}
}