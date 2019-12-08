using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IAccountCondition
	{
		DateTime? IfModifiedSinceTime
		{
			get;
		}

		DateTime? IfNotModifiedSinceTime
		{
			get;
		}

		bool IncludeDisabledAccounts
		{
			get;
		}

		bool IncludeExpiredAccounts
		{
			get;
		}
	}
}