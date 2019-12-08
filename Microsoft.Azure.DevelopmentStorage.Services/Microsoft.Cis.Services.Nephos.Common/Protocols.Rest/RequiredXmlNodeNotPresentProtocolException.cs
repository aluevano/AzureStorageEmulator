using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class RequiredXmlNodeNotPresentProtocolException : ProtocolException
	{
		private string xmlNodeName;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.MissingRequiredXmlNode;
			}
		}

		public string XmlNodeName
		{
			get
			{
				return this.xmlNodeName;
			}
			set
			{
				this.xmlNodeName = value;
			}
		}

		public RequiredXmlNodeNotPresentProtocolException(string xmlNodeName) : this(xmlNodeName, (Exception)null)
		{
		}

		public RequiredXmlNodeNotPresentProtocolException(string xmlNodeName, string message) : base(message)
		{
			this.xmlNodeName = xmlNodeName;
		}

		public RequiredXmlNodeNotPresentProtocolException(string xmlNodeName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The xml node {0} must be supplied", new object[] { xmlNodeName }), innerException)
		{
			this.xmlNodeName = xmlNodeName;
		}

		public RequiredXmlNodeNotPresentProtocolException(string xmlNodeName, string message, Exception innerException) : base(message, innerException)
		{
			this.xmlNodeName = xmlNodeName;
		}

		protected RequiredXmlNodeNotPresentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.xmlNodeName = info.GetString("this.xmlNodeName");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "XmlNodeName", this.XmlNodeName }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.xmlNodeName", this.xmlNodeName);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new RequiredXmlNodeNotPresentProtocolException(this.xmlNodeName, this.Message, this);
		}
	}
}