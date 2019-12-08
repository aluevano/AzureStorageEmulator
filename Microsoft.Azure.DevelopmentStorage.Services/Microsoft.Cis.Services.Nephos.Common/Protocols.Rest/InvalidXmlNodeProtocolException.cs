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
	public class InvalidXmlNodeProtocolException : ProtocolException
	{
		private string xmlNodeName;

		private string xmlNodeValue;

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidXmlNodeValue;
			}
		}

		public string XmlNodeName
		{
			get
			{
				return this.xmlNodeName;
			}
		}

		public string XmlNodeValue
		{
			get
			{
				return this.xmlNodeValue;
			}
		}

		public InvalidXmlNodeProtocolException(string xmlNodeName, string xmlNodeValue) : this(xmlNodeName, xmlNodeValue, (Exception)null)
		{
		}

		public InvalidXmlNodeProtocolException(string xmlNodeName, string xmlNodeValue, string message) : base(message)
		{
			this.xmlNodeName = xmlNodeName;
			this.xmlNodeValue = xmlNodeValue;
		}

		public InvalidXmlNodeProtocolException(string xmlNodeName, string xmlNodeValue, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The value {0} provided for XML node {1} is invalid.", new object[] { xmlNodeValue, xmlNodeName }), innerException)
		{
			this.xmlNodeName = xmlNodeName;
			this.xmlNodeValue = xmlNodeValue;
		}

		public InvalidXmlNodeProtocolException(string xmlNodeName, string xmlNodeValue, string message, Exception innerException) : base(message, innerException)
		{
			this.xmlNodeName = xmlNodeName;
			this.xmlNodeValue = xmlNodeValue;
		}

		protected InvalidXmlNodeProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.xmlNodeName = info.GetString("this.xmlNodeName");
			this.xmlNodeValue = info.GetString("this.xmlNodeValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(2)
			{
				{ "XmlNodeName", this.XmlNodeName },
				{ "XmlNodeValue", this.XmlNodeValue }
			};
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.xmlNodeName", this.xmlNodeName);
			info.AddValue("this.xmlNodeValue", this.xmlNodeValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidXmlNodeProtocolException(this.XmlNodeName, this.XmlNodeValue, this);
		}
	}
}