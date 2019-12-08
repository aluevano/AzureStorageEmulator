using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface ILeaseInfo
	{
		TimeSpan? Duration
		{
			get;
		}

		DateTime? EndTime
		{
			get;
		}

		Guid? Id
		{
			get;
		}

		LeaseState? State
		{
			get;
		}

		LeaseType Type
		{
			get;
		}
	}
}