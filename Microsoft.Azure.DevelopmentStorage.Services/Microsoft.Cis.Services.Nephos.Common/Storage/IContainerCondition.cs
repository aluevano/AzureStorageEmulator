using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public interface IContainerCondition
	{
		DateTime? IfModifiedSinceTime
		{
			get;
		}

		DateTime? IfNotModifiedSinceTime
		{
			get;
		}

		bool IncludeDisabledContainers
		{
			get;
		}

		bool IncludeExpiredContainers
		{
			get;
		}

		bool IncludeSnapshots
		{
			get;
		}

		bool? IsDeletingOnlySnapshots
		{
			get;
		}

		bool? IsRequiringNoSnapshots
		{
			get;
		}

		Guid? LeaseId
		{
			get;
		}

		DateTime? SnapshotTimestamp
		{
			get;
		}
	}
}