using AsyncHelper;
using System;
using System.Collections.Specialized;
using System.Runtime.Serialization;

namespace Microsoft.Cis.Services.Nephos.Common.Authentication
{
	[Serializable]
	public class DstsAuthenticationFailureException : Exception, IRethrowableException
	{
		private const string dstsAuthHeaderName = "WWW-Authenticate-dSTS";

		private string dstsAuthHeaderValue;

		public NameValueCollection Headers
		{
			get
			{
				return new NameValueCollection()
				{
					{ "WWW-Authenticate-dSTS", this.dstsAuthHeaderValue }
				};
			}
		}

		public DstsAuthenticationFailureException(string header)
		{
			this.dstsAuthHeaderValue = header;
		}

		public DstsAuthenticationFailureException(string header, string message) : base(message)
		{
			this.dstsAuthHeaderValue = header;
		}

		public DstsAuthenticationFailureException(string header, string message, Exception innerException) : base(message, innerException)
		{
			this.dstsAuthHeaderValue = header;
		}

		protected DstsAuthenticationFailureException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.dstsAuthHeaderValue = info.GetString("this.DstsAuthHeaderValue");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.DstsAuthHeaderValue", this.dstsAuthHeaderValue);
			base.GetObjectData(info, context);
		}

		public virtual Exception GetRethrowableClone()
		{
			return new DstsAuthenticationFailureException(this.dstsAuthHeaderValue, base.Message, this);
		}
	}
}