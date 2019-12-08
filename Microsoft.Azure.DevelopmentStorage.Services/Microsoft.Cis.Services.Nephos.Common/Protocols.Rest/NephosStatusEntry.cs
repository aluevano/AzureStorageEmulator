using System;
using System.Net;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class NephosStatusEntry : ISerializable
	{
		private readonly string statusId;

		private readonly HttpStatusCode statusCodeHttp;

		private readonly string userMessage;

		public HttpStatusCode StatusCodeHttp
		{
			get
			{
				return this.statusCodeHttp;
			}
		}

		public string StatusId
		{
			get
			{
				return this.statusId;
			}
		}

		public string UserMessage
		{
			get
			{
				return this.userMessage;
			}
		}

		public NephosStatusEntry(string statusId, HttpStatusCode statusCode, string userMessage)
		{
			this.statusId = statusId;
			this.statusCodeHttp = statusCode;
			this.userMessage = userMessage;
		}

		protected NephosStatusEntry(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.statusId = info.GetString("this.statusId");
			this.statusCodeHttp = (HttpStatusCode)info.GetValue("this.statusCodeHttp", typeof(HttpStatusCode));
			this.userMessage = info.GetString("this.userMessage");
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.statusId", this.statusId);
			info.AddValue("this.statusCodeHttp", this.statusCodeHttp);
			info.AddValue("this.userMessage", this.userMessage);
		}
	}
}