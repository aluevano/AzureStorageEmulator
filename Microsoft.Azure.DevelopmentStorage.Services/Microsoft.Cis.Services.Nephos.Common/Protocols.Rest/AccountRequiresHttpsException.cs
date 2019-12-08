using System;
using System.Collections.Specialized;
using System.Globalization;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	[Serializable]
	public class AccountRequiresHttpsException : ProtocolException
	{
		private string accountName;

		public string AccountName
		{
			get
			{
				return this.accountName;
			}
			set
			{
				this.accountName = value;
			}
		}

		public override NephosStatusEntry StatusEntry
		{
			get
			{
				return CommonStatusEntries.AccountRequiresHttps;
			}
		}

		public AccountRequiresHttpsException(string accountName, Exception innerException) : base(string.Format(CultureInfo.InvariantCulture, "http requests not supported for the storage account {0}, since supportsHttpsTrafficOnly flag is set on the account.", new object[] { accountName }), innerException)
		{
			this.accountName = accountName;
		}

		public AccountRequiresHttpsException(string accountName, string message, Exception innerException) : base(message, innerException)
		{
			this.accountName = accountName;
		}

		protected AccountRequiresHttpsException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			this.accountName = info.GetString("this.accountName");
		}

		public override NameValueCollection GetAdditionalUserDetails()
		{
			return new NameValueCollection(1)
			{
				{ "AccountName", this.AccountName }
			};
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}
			info.AddValue("this.accountName", this.accountName);
			base.GetObjectData(info, context);
		}

		public override Exception GetRethrowableClone()
		{
			return new AccountRequiresHttpsException(this.AccountName, this.Message, this);
		}
	}
}