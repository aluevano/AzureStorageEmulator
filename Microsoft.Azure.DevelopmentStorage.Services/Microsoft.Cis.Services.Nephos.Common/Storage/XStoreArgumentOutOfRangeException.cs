using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class XStoreArgumentOutOfRangeException : StorageManagerException
	{
		public string UserResponseMessage
		{
			get;
			private set;
		}

		public XStoreArgumentOutOfRangeException()
		{
			this.UserResponseMessage = "One of the request inputs is out of range.";
		}

		public XStoreArgumentOutOfRangeException(string message) : base(message)
		{
			this.UserResponseMessage = "One of the request inputs is out of range.";
		}

		public XStoreArgumentOutOfRangeException(string paramName, string message) : this(paramName, message, "One of the request inputs is out of range.")
		{
		}

		public XStoreArgumentOutOfRangeException(string paramName, string message, string userResponseMessage) : base(message, new ArgumentOutOfRangeException(paramName, message))
		{
			this.UserResponseMessage = userResponseMessage;
		}

		public XStoreArgumentOutOfRangeException(string message, Exception innerException) : this(message, "One of the request inputs is out of range.", innerException)
		{
		}

		public XStoreArgumentOutOfRangeException(string message, string userResponseMessage, Exception innerException) : base(message, innerException)
		{
			this.UserResponseMessage = userResponseMessage;
		}

		protected XStoreArgumentOutOfRangeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.UserResponseMessage = info.GetString("this.UserResponseMessage");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("this.UserResponseMessage", this.UserResponseMessage);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new XStoreArgumentOutOfRangeException(this.Message, this.UserResponseMessage, this);
		}
	}
}