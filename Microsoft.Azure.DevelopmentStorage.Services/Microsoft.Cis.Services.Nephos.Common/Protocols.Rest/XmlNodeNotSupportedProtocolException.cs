using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the xmlNode name and value must be provided.")]
	public class XmlNodeNotSupportedProtocolException : ProtocolException
	{
		private string xmlNodeName;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.UnsupportedXmlNode;
			}
		}

		public string XmlNodeName
		{
			get
			{
				return this.xmlNodeName;
			}
		}

		public XmlNodeNotSupportedProtocolException(string xmlNodeName, string message) : base(message)
		{
			this.xmlNodeName = xmlNodeName;
		}

		public XmlNodeNotSupportedProtocolException(string xmlNodeName) : this(xmlNodeName, (Exception)null)
		{
		}

		public XmlNodeNotSupportedProtocolException(string xmlNodeName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The XML node '{0}' is not supported for the given operation.", new object[] { xmlNodeName }), innerException)
		{
			this.xmlNodeName = xmlNodeName;
		}

		protected XmlNodeNotSupportedProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
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
			return new XmlNodeNotSupportedProtocolException(this.XmlNodeName, this);
		}
	}
}