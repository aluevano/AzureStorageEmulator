using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the query parameter name and value must be provided.")]
	public class OutOfRangeXmlArgumentProtocolException : ProtocolException
	{
		private string xmlArgumentName;

		private string xmlArgumentValue;

		private string minValue;

		private string maxValue;

		public string MaxValue
		{
			get
			{
				return this.maxValue;
			}
		}

		public string MinValue
		{
			get
			{
				return this.minValue;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.OutOfRangeXmlArgument;
			}
		}

		public string XmlArgumentName
		{
			get
			{
				return this.xmlArgumentName;
			}
		}

		public string XmlArgumentValue
		{
			get
			{
				return this.xmlArgumentValue;
			}
		}

		public OutOfRangeXmlArgumentProtocolException(string name, string value, string min, string max) : this(name, value, min, max, null)
		{
		}

		public OutOfRangeXmlArgumentProtocolException(string name, string value, string min, string max, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The value {0} of XML argument {1} is out of range. Allowed range is from {2} to {3}.", new object[] { value, name, min, max }), innerException)
		{
			this.xmlArgumentName = name;
			this.xmlArgumentValue = value;
			this.minValue = min;
			this.maxValue = max;
		}

		protected OutOfRangeXmlArgumentProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.xmlArgumentName = info.GetString("this.xmlArgumentName");
			this.xmlArgumentValue = info.GetString("this.xmlArgumentValue");
			this.minValue = info.GetString("this.minValue");
			this.maxValue = info.GetString("this.maxValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(4)
			{
				{ "XmlArgumentName", this.XmlArgumentName },
				{ "XmlArgumentValue", this.XmlArgumentValue },
				{ "MinimumAllowed", this.MinValue },
				{ "MaximumAllowed", this.MaxValue }
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
			info.AddValue("this.xmlArgumentName", this.xmlArgumentName);
			info.AddValue("this.xmlArgumentValue", this.xmlArgumentValue);
			info.AddValue("this.minValue", this.minValue);
			info.AddValue("this.maxValue", this.maxValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new OutOfRangeXmlArgumentProtocolException(this.XmlArgumentName, this.XmlArgumentValue, this.MinValue, this.MaxValue, this);
		}
	}
}