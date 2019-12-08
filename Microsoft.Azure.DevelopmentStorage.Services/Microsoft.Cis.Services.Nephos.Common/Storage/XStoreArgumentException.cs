using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	[Serializable]
	public class XStoreArgumentException : StorageManagerException
	{
		public string UserResponseMessage
		{
			get;
			private set;
		}

		public XStoreArgumentException()
		{
		}

		public XStoreArgumentException(string message) : this(message, (Exception)null)
		{
		}

		public XStoreArgumentException(string message, string userResponseMessage) : this(message, userResponseMessage, null)
		{
		}

		public XStoreArgumentException(string message, Exception e) : this(message, null, e)
		{
		}

		public XStoreArgumentException(string message, string userResponseMessage, Exception exception) : base(message, exception)
		{
			this.UserResponseMessage = userResponseMessage;
		}

		protected XStoreArgumentException(SerializationInfo info, StreamingContext context) : base(info, context)
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
			return new XStoreArgumentException(this.Message, this.UserResponseMessage, this);
		}
	}
}