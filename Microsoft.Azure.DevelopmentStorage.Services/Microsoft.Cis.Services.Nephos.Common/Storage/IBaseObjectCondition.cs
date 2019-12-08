using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IBaseObjectCondition
	{
		DateTime[] IfLastModificationTimeMatch
		{
			get;
		}

		DateTime[] IfLastModificationTimeMismatch
		{
			get;
		}

		DateTime? IfModifiedSinceTime
		{
			get;
		}

		DateTime? IfNotModifiedSinceTime
		{
			get;
		}

		bool IsMultipleConditionalHeaderEnabled
		{
			get;
			set;
		}
	}
}