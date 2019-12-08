using AsyncHelper;
using System;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	public class TableServiceArgumentException : Exception, IRethrowableException
	{
		public TableServiceArgumentException()
		{
		}

		public TableServiceArgumentException(string message) : base(message)
		{
		}

		public TableServiceArgumentException(Exception e) : base(string.Empty, e)
		{
		}

		public TableServiceArgumentException(string message, Exception e) : base(message, e)
		{
		}

		protected TableServiceArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
		}

		public Exception GetRethrowableClone()
		{
			return new TableServiceArgumentException(this.Message, this);
		}
	}
}