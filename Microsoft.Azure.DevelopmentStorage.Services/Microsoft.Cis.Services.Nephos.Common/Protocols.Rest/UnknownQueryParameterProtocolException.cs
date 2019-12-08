using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the query parameter name must be provided.")]
	public class UnknownQueryParameterProtocolException : ProtocolException
	{
		private string queryParamName;

		public string QueryParamName
		{
			get
			{
				return this.queryParamName;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.UnsupportedQueryParameter;
			}
		}

		public UnknownQueryParameterProtocolException(string queryParamName) : this(queryParamName, null)
		{
		}

		public UnknownQueryParameterProtocolException(string queryParamName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "The query parameter {0} is unknown/not supported for this URL", new object[] { queryParamName }), innerException)
		{
			this.queryParamName = queryParamName;
		}

		protected UnknownQueryParameterProtocolException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.queryParamName = info.GetString("this.queryParamName");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "QueryParameterName", this.QueryParamName }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.queryParamName", this.queryParamName);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new UnknownQueryParameterProtocolException(this.QueryParamName, this);
		}
	}
}