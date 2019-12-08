using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class TableBatchDuplicateRowKeyException : StorageManagerException
	{
		public TableBatchDuplicateRowKeyException(string message) : base(message)
		{
		}

		public TableBatchDuplicateRowKeyException(string message, Exception innerException) : base(message, innerException)
		{
		}

		protected TableBatchDuplicateRowKeyException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new TableBatchDuplicateRowKeyException(this.Message, this);
		}
	}
}