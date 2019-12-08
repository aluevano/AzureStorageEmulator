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
	public class InvalidQueryParameterProtocolException : ProtocolException
	{
		private string queryParamName;

		private string queryParamValue;

		private string reason;

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

		public string Reason
		{
			get
			{
				return this.reason;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.InvalidQueryParameterValue;
			}
		}

		public InvalidQueryParameterProtocolException(string queryParamName, string queryParamValue, string reason) : this(queryParamName, queryParamValue, reason, null)
		{
		}

		public InvalidQueryParameterProtocolException(string queryParamName, string queryParamValue, string reason, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The value {0} provided for query parameter {1} is invalid. {2}", new object[] { queryParamValue, queryParamName, reason }), innerException)
		{
			this.queryParamName = queryParamName;
			this.queryParamValue = queryParamValue;
			this.reason = reason;
		}

		protected InvalidQueryParameterProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.queryParamName = info.GetString("this.queryParamName");
			this.queryParamValue = info.GetString("this.queryParamValue");
			this.reason = info.GetString("this.reason");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = new NameValueCollection(2)
			{
				{ "QueryParameterName", this.QueryParamName },
				{ "QueryParameterValue", this.QueryParamValue },
				{ "Reason", this.Reason }
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
			info.AddValue("this.reason", this.reason);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new InvalidQueryParameterProtocolException(this.QueryParamName, this.QueryParamValue, this.Reason, this);
		}
	}
}