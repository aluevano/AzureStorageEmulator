using System;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	[SuppressMessage("Microsoft.Design", "CA1032:ImplementStandardExceptionConstructors", Justification="The default constructor taking no arguments is explicitly omitted since the maximum limit must be provided.")]
	public class RequestEntityTooLargeException : ProtocolException
	{
		private long? maxLimit;

		public long? MaxLimit
		{
			get
			{
				return this.maxLimit;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.RequestBodyTooLarge;
			}
		}

		public RequestEntityTooLargeException(long? maxLimit) : this(maxLimit, null)
		{
		}

		public RequestEntityTooLargeException(long? maxLimit, Exception innerException) : base((string.Concat("The request entity is too large. ", maxLimit) != null ? string.Format(CultureInfo.InvariantCulture, "Max limit is {0}", new object[] { maxLimit.Value }) : ""), innerException)
		{
			this.maxLimit = maxLimit;
		}

		protected RequestEntityTooLargeException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.maxLimit = (long?)info.GetValue("this.maxLimit", typeof(long?));
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			NameValueCollection nameValueCollection = null;
			if (this.MaxLimit.HasValue)
			{
				nameValueCollection = new NameValueCollection(1);
				long value = this.MaxLimit.Value;
				nameValueCollection.Add("MaxLimit", value.ToString(CultureInfo.InvariantCulture));
			}
			return nameValueCollection;
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.maxLimit", this.maxLimit);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new RequestEntityTooLargeException(this.MaxLimit, this);
		}
	}
}