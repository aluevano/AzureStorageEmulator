using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class PageRangeInvalidException : StorageManagerException
	{
		public PageRangeInvalidException() : base("The specified page range is invalid.")
		{
		}

		public PageRangeInvalidException(string message, int xstoreErrorCode) : base(message, xstoreErrorCode)
		{
		}

		public PageRangeInvalidException(string message, int xstoreErrorCode, Exception innerException) : base(message, xstoreErrorCode, innerException)
		{
		}

		protected PageRangeInvalidException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public override Exception GetRethrowableClone()
		{
			return new PageRangeInvalidException(this.Message, base.XStoreErrorCode, this);
		}
	}
}