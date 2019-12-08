using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class BlockCountExceedsLimitException : StorageManagerException
	{
		public BlockCountExceedsLimitException() : base("The block count exceeds the maximum permissible limit.")
		{
		}

		public BlockCountExceedsLimitException(string message) : base(message)
		{
		}

		public BlockCountExceedsLimitException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected BlockCountExceedsLimitException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new BlockCountExceedsLimitException(this.Message, this);
		}
	}
}