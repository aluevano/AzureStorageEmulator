using AsyncHelper;
using System;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	[Serializable]
	public class CannotVerifyCopySourceException : Exception, IRethrowableException
	{
		public HttpStatusCode StatusCode
		{
			get;
			private set;
		}

		public string StatusDescription
		{
			get;
			private set;
		}

		public CannotVerifyCopySourceException() : this(null)
		{
		}

		public CannotVerifyCopySourceException(Exception innerException) : this(HttpStatusCode.InternalServerError, "Could not verify copy source.", innerException)
		{
		}

		public CannotVerifyCopySourceException(HttpStatusCode statusCode, string message) : this(statusCode, message, null)
		{
		}

		public CannotVerifyCopySourceException(HttpStatusCode statusCode, string message, Exception innerException) : base(message, innerException)
		{
			this.StatusCode = statusCode;
			this.StatusDescription = message;
		}

		public CannotVerifyCopySourceException(string message, CannotVerifyCopySourceException innerException) : base(message, innerException)
		{
			if (innerException == null)
			{
				throw new ArgumentNullException("innerException");
			}
			this.StatusCode = innerException.StatusCode;
			this.StatusDescription = innerException.StatusDescription;
		}

		protected CannotVerifyCopySourceException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.StatusCode = (HttpStatusCode)info.GetInt32("this.StatusCode");
			this.StatusDescription = info.GetString("this.StatusDescription");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.StatusCode", (int)this.StatusCode);
			info.AddValue("this.StatusDescription", this.StatusDescription);
			base.GetObjectData(info, context);
		}

		public virtual Exception GetRethrowableClone()
		{
			return new CannotVerifyCopySourceException(this.Message, this);
		}
	}
}