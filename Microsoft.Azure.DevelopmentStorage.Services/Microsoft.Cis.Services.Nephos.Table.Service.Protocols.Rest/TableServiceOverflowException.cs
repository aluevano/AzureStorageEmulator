using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	public class TableServiceOverflowException : Exception, IRethrowableException
	{
		public TableServiceOverflowException()
		{
		}

		public TableServiceOverflowException(string message) : base(message)
		{
		}

		public TableServiceOverflowException(Exception e) : base(string.Empty, e)
		{
		}

		public TableServiceOverflowException(string message, Exception e) : base(message, e)
		{
		}

		protected TableServiceOverflowException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new TableServiceOverflowException(this.Message, this);
		}
	}
}