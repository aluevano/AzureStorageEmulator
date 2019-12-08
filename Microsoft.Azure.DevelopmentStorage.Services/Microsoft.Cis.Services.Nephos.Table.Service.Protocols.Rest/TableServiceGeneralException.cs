using AsyncHelper;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	[Serializable]
	public class TableServiceGeneralException : Exception, IRethrowableException
	{
		public TableServiceError ErrorCode
		{
			get;
			private set;
		}

		public TableServiceGeneralException()
		{
		}

		public TableServiceGeneralException(TableServiceError errorCode, Exception innerException) : base("", innerException)
		{
			this.ErrorCode = errorCode;
		}

		public TableServiceGeneralException(string message) : base(message)
		{
		}

		public TableServiceGeneralException(Exception e) : base(string.Empty, e)
		{
		}

		public TableServiceGeneralException(string message, Exception e) : base(message, e)
		{
		}

		protected TableServiceGeneralException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.ErrorCode = (TableServiceError)info.GetValue("this.ErrorCode", typeof(TableServiceError));
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.ErrorCode", this.ErrorCode);
			base.GetObjectData(info, context);
		}

		public Exception GetRethrowableClone()
		{
			return new TableServiceGeneralException(this.ErrorCode, this);
		}
	}
}