using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class LeaseNotPresentException : StorageManagerException
	{
		public LeaseNotPresentException() : base("The blob has no lease.")
		{
		}

		public LeaseNotPresentException(string message) : base(message)
		{
		}

		public LeaseNotPresentException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected LeaseNotPresentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new LeaseNotPresentException(this.Message, this);
		}
	}
}