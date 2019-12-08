using System;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class AccountCondition : IAccountCondition
	{
		private bool includeDisabledAccounts;

		private bool includeExpiredAccounts;

		private DateTime? ifModifiedSinceTime;

		private DateTime? ifNotModifiedSinceTime;

		public DateTime? IfModifiedSinceTime
		{
			get
			{
				return JustDecompileGenerated_get_IfModifiedSinceTime();
			}
			set
			{
				JustDecompileGenerated_set_IfModifiedSinceTime(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_IfModifiedSinceTime()
		{
			return this.ifModifiedSinceTime;
		}

		public void JustDecompileGenerated_set_IfModifiedSinceTime(DateTime? value)
		{
			this.ifModifiedSinceTime = value;
		}

		public DateTime? IfNotModifiedSinceTime
		{
			get
			{
				return JustDecompileGenerated_get_IfNotModifiedSinceTime();
			}
			set
			{
				JustDecompileGenerated_set_IfNotModifiedSinceTime(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_IfNotModifiedSinceTime()
		{
			return this.ifNotModifiedSinceTime;
		}

		public void JustDecompileGenerated_set_IfNotModifiedSinceTime(DateTime? value)
		{
			this.ifNotModifiedSinceTime = value;
		}

		public bool IncludeDisabledAccounts
		{
			get
			{
				return JustDecompileGenerated_get_IncludeDisabledAccounts();
			}
			set
			{
				JustDecompileGenerated_set_IncludeDisabledAccounts(value);
			}
		}

		public bool JustDecompileGenerated_get_IncludeDisabledAccounts()
		{
			return this.includeDisabledAccounts;
		}

		public void JustDecompileGenerated_set_IncludeDisabledAccounts(bool value)
		{
			this.includeDisabledAccounts = value;
		}

		public bool IncludeExpiredAccounts
		{
			get
			{
				return JustDecompileGenerated_get_IncludeExpiredAccounts();
			}
			set
			{
				JustDecompileGenerated_set_IncludeExpiredAccounts(value);
			}
		}

		public bool JustDecompileGenerated_get_IncludeExpiredAccounts()
		{
			return this.includeExpiredAccounts;
		}

		public void JustDecompileGenerated_set_IncludeExpiredAccounts(bool value)
		{
			this.includeExpiredAccounts = value;
		}

		public AccountCondition()
		{
		}

		public AccountCondition(bool includeDisabledAccounts, bool includeExpiredAccounts, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime) : this()
		{
			this.includeDisabledAccounts = includeDisabledAccounts;
			this.includeExpiredAccounts = includeExpiredAccounts;
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
		}

		public override string ToString()
		{
			string empty = string.Empty;
			string str = string.Empty;
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] includeDisabledAccounts = new object[] { this.IncludeDisabledAccounts, this.IncludeExpiredAccounts, empty, str, null, null };
			includeDisabledAccounts[4] = (this.IfModifiedSinceTime.HasValue ? this.IfModifiedSinceTime.Value.ToString("O") : "<null>");
			includeDisabledAccounts[5] = (this.IfNotModifiedSinceTime.HasValue ? this.IfNotModifiedSinceTime.Value.ToString("O") : "<null>");
			return string.Format(invariantCulture, "AccountCondition(IncludeDisabledAccounts={0}, IncludeExpiredAccounts={1}, {2}{3}IfModifiedSinceTime={4}, IfNotModifiedSinceTime={5})", includeDisabledAccounts);
		}
	}
}