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
	public class OutOfRangeXmlNodeCountProtocolException : ProtocolException
	{
		private string xmlNodeCountName;

		private string xmlNodeCountValue;

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
				return CommonStatusEntries.OutOfRangeXmlNodeCount;
			}
		}

		public string XmlNodeCountName
		{
			get
			{
				return this.xmlNodeCountName;
			}
		}

		public string XmlNodeCountValue
		{
			get
			{
				return this.xmlNodeCountValue;
			}
		}

		public OutOfRangeXmlNodeCountProtocolException(string name, string value, string min, string max) : this(name, value, min, max, null)
		{
		}

		public OutOfRangeXmlNodeCountProtocolException(string name, string value, string min, string max, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The count {0} of XML node {1} is out of range. Allowed range is from {2} to {3}.", new object[] { value, name, min, max }), innerException)
		{
			this.xmlNodeCountName = name;
			this.xmlNodeCountValue = value;
			this.minValue = min;
			this.maxValue = max;
		}

		protected OutOfRangeXmlNodeCountProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.xmlNodeCountName = info.GetString("this.xmlNodeCountName");
			this.xmlNodeCountValue = info.GetString("this.xmlNodeCountValue");
			this.minValue = info.GetString("this.minValue");
			this.maxValue = info.GetString("this.maxValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(4)
			{
				{ "XmlNodeCountName", this.XmlNodeCountName },
				{ "XmlNodeCountValue", this.XmlNodeCountValue },
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
			info.AddValue("this.xmlNodeCountName", this.xmlNodeCountName);
			info.AddValue("this.xmlNodeCountValue", this.xmlNodeCountValue);
			info.AddValue("this.minValue", this.minValue);
			info.AddValue("this.maxValue", this.maxValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new OutOfRangeXmlNodeCountProtocolException(this.XmlNodeCountName, this.XmlNodeCountValue, this.MinValue, this.MaxValue, this);
		}
	}
}