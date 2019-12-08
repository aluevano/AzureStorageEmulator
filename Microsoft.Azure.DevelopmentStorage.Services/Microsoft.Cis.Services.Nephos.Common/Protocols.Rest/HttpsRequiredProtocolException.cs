using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class HttpsRequiredProtocolException : ProtocolException
	{
		private string commandName;

		public string CommandName
		{
			get
			{
				return this.commandName;
			}
			set
			{
				this.commandName = value;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.HttpsRequired;
			}
		}

		public HttpsRequiredProtocolException(string commandName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The command {0} must be sent using https", new object[] { commandName }), innerException)
		{
			this.commandName = commandName;
		}

		public HttpsRequiredProtocolException(string commandName, string message, Exception innerException) : base(message, innerException)
		{
			this.commandName = commandName;
		}

		protected HttpsRequiredProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.commandName = info.GetString("this.commandName");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "CommandName", this.CommandName }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.commandName", this.commandName);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new HttpsRequiredProtocolException(this.commandName, this.Message, this);
		}
	}
}