using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	public class TableServiceArgumentOutOfRangeException : Exception, IRethrowableException
	{
		public TableServiceArgumentOutOfRangeException()
		{
		}

		public TableServiceArgumentOutOfRangeException(string message) : base(message)
		{
		}

		public TableServiceArgumentOutOfRangeException(Exception e) : base(string.Empty, e)
		{
		}

		public TableServiceArgumentOutOfRangeException(string message, Exception e) : base(message, e)
		{
		}

		protected TableServiceArgumentOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new TableServiceArgumentOutOfRangeException(this.Message, this);
		}
	}
}