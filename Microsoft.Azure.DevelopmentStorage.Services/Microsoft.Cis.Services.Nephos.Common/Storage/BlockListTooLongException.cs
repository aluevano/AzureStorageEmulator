using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlockListTooLongException : StorageManagerException
	{
		public BlockListTooLongException() : base("The block list may not contain more than 50,000 blocks.")
		{
		}

		public BlockListTooLongException(string message) : base(message)
		{
		}

		public BlockListTooLongException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlockListTooLongException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlockListTooLongException(this.Message, this);
		}
	}
}