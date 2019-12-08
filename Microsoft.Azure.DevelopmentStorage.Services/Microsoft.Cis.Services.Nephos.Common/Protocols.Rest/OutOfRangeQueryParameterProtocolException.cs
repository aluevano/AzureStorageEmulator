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
	public class OutOfRangeQueryParameterProtocolException : ProtocolException
	{
		private string queryParamName;

		private string queryParamValue;

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

		public string QueryParamName
		{
			get
			{
				return this.queryParamName;
			}
		}

		public string QueryParamValue
		{
			get
			{
				return this.queryParamValue;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.OutOfRangeQueryParameterValue;
			}
		}

		public OutOfRangeQueryParameterProtocolException(string name, string value, string min, string max) : this(name, value, min, max, null)
		{
		}

		public OutOfRangeQueryParameterProtocolException(string name, string value, string min, string max, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The value {0} specified for query parameter {1} is out of range. Allowed range is from {2} to {3}.", new object[] { value, name, min, max }), innerException)
		{
			this.queryParamName = name;
			this.queryParamValue = value;
			this.minValue = min;
			this.maxValue = max;
		}

		protected OutOfRangeQueryParameterProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.queryParamName = info.GetString("this.queryParamName");
			this.queryParamValue = info.GetString("this.queryParamValue");
			this.minValue = info.GetString("this.minValue");
			this.maxValue = info.GetString("this.maxValue");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(4)
			{
				{ "QueryParameterName", this.QueryParamName },
				{ "QueryParameterValue", this.QueryParamValue },
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
			info.AddValue("this.queryParamName", this.queryParamName);
			info.AddValue("this.queryParamValue", this.queryParamValue);
			info.AddValue("this.minValue", this.minValue);
			info.AddValue("this.maxValue", this.maxValue);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new OutOfRangeQueryParameterProtocolException(this.QueryParamName, this.QueryParamValue, this.MinValue, this.MaxValue, this);
		}
	}
}