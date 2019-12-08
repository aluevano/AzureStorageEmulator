using System;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class ContainerCondition : IContainerCondition
	{
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

		private DateTime? JustDecompileGenerated_IfModifiedSinceTime_k__BackingField;

		public DateTime? JustDecompileGenerated_get_IfModifiedSinceTime()
		{
			return this.JustDecompileGenerated_IfModifiedSinceTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_IfModifiedSinceTime(DateTime? value)
		{
			this.JustDecompileGenerated_IfModifiedSinceTime_k__BackingField = value;
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

		private DateTime? JustDecompileGenerated_IfNotModifiedSinceTime_k__BackingField;

		public DateTime? JustDecompileGenerated_get_IfNotModifiedSinceTime()
		{
			return this.JustDecompileGenerated_IfNotModifiedSinceTime_k__BackingField;
		}

		public void JustDecompileGenerated_set_IfNotModifiedSinceTime(DateTime? value)
		{
			this.JustDecompileGenerated_IfNotModifiedSinceTime_k__BackingField = value;
		}

		public bool IncludeDisabledContainers
		{
			get
			{
				return JustDecompileGenerated_get_IncludeDisabledContainers();
			}
			set
			{
				JustDecompileGenerated_set_IncludeDisabledContainers(value);
			}
		}

		private bool JustDecompileGenerated_IncludeDisabledContainers_k__BackingField;

		public bool JustDecompileGenerated_get_IncludeDisabledContainers()
		{
			return this.JustDecompileGenerated_IncludeDisabledContainers_k__BackingField;
		}

		public void JustDecompileGenerated_set_IncludeDisabledContainers(bool value)
		{
			this.JustDecompileGenerated_IncludeDisabledContainers_k__BackingField = value;
		}

		public bool IncludeExpiredContainers
		{
			get
			{
				return JustDecompileGenerated_get_IncludeExpiredContainers();
			}
			set
			{
				JustDecompileGenerated_set_IncludeExpiredContainers(value);
			}
		}

		private bool JustDecompileGenerated_IncludeExpiredContainers_k__BackingField;

		public bool JustDecompileGenerated_get_IncludeExpiredContainers()
		{
			return this.JustDecompileGenerated_IncludeExpiredContainers_k__BackingField;
		}

		public void JustDecompileGenerated_set_IncludeExpiredContainers(bool value)
		{
			this.JustDecompileGenerated_IncludeExpiredContainers_k__BackingField = value;
		}

		public bool IncludeSnapshots
		{
			get
			{
				return JustDecompileGenerated_get_IncludeSnapshots();
			}
			set
			{
				JustDecompileGenerated_set_IncludeSnapshots(value);
			}
		}

		private bool JustDecompileGenerated_IncludeSnapshots_k__BackingField;

		public bool JustDecompileGenerated_get_IncludeSnapshots()
		{
			return this.JustDecompileGenerated_IncludeSnapshots_k__BackingField;
		}

		public void JustDecompileGenerated_set_IncludeSnapshots(bool value)
		{
			this.JustDecompileGenerated_IncludeSnapshots_k__BackingField = value;
		}

		public bool? IsDeletingOnlySnapshots
		{
			get
			{
				return JustDecompileGenerated_get_IsDeletingOnlySnapshots();
			}
			set
			{
				JustDecompileGenerated_set_IsDeletingOnlySnapshots(value);
			}
		}

		private bool? JustDecompileGenerated_IsDeletingOnlySnapshots_k__BackingField;

		public bool? JustDecompileGenerated_get_IsDeletingOnlySnapshots()
		{
			return this.JustDecompileGenerated_IsDeletingOnlySnapshots_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsDeletingOnlySnapshots(bool? value)
		{
			this.JustDecompileGenerated_IsDeletingOnlySnapshots_k__BackingField = value;
		}

		public bool? IsRequiringNoSnapshots
		{
			get
			{
				return JustDecompileGenerated_get_IsRequiringNoSnapshots();
			}
			set
			{
				JustDecompileGenerated_set_IsRequiringNoSnapshots(value);
			}
		}

		private bool? JustDecompileGenerated_IsRequiringNoSnapshots_k__BackingField;

		public bool? JustDecompileGenerated_get_IsRequiringNoSnapshots()
		{
			return this.JustDecompileGenerated_IsRequiringNoSnapshots_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsRequiringNoSnapshots(bool? value)
		{
			this.JustDecompileGenerated_IsRequiringNoSnapshots_k__BackingField = value;
		}

		public Guid? LeaseId
		{
			get
			{
				return JustDecompileGenerated_get_LeaseId();
			}
			set
			{
				JustDecompileGenerated_set_LeaseId(value);
			}
		}

		private Guid? JustDecompileGenerated_LeaseId_k__BackingField;

		public Guid? JustDecompileGenerated_get_LeaseId()
		{
			return this.JustDecompileGenerated_LeaseId_k__BackingField;
		}

		public void JustDecompileGenerated_set_LeaseId(Guid? value)
		{
			this.JustDecompileGenerated_LeaseId_k__BackingField = value;
		}

		public DateTime? SnapshotTimestamp
		{
			get
			{
				return JustDecompileGenerated_get_SnapshotTimestamp();
			}
			set
			{
				JustDecompileGenerated_set_SnapshotTimestamp(value);
			}
		}

		private DateTime? JustDecompileGenerated_SnapshotTimestamp_k__BackingField;

		public DateTime? JustDecompileGenerated_get_SnapshotTimestamp()
		{
			return this.JustDecompileGenerated_SnapshotTimestamp_k__BackingField;
		}

		public void JustDecompileGenerated_set_SnapshotTimestamp(DateTime? value)
		{
			this.JustDecompileGenerated_SnapshotTimestamp_k__BackingField = value;
		}

		public ContainerCondition()
		{
		}

		public ContainerCondition(bool includeDisabledContainers, bool includeExpiredContainers, bool includeSnapshots, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, Guid? leaseId)
		{
			this.IncludeDisabledContainers = includeDisabledContainers;
			this.IncludeExpiredContainers = includeExpiredContainers;
			this.IncludeSnapshots = includeSnapshots;
			this.IfModifiedSinceTime = ifModifiedSinceTime;
			this.IfNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.LeaseId = leaseId;
		}

		public ContainerCondition(bool includeDisabledContainers, bool includeExpiredContainers, bool includeSnapshots, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, DateTime? snapshotTimestamp, bool? isRequiringNoSnapshots, bool? isDeletingOnlySnapshots, Guid? leaseId)
		{
			this.IncludeDisabledContainers = includeDisabledContainers;
			this.IncludeExpiredContainers = includeExpiredContainers;
			this.IncludeSnapshots = includeSnapshots;
			this.IfModifiedSinceTime = ifModifiedSinceTime;
			this.IfNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.SnapshotTimestamp = snapshotTimestamp;
			this.IsRequiringNoSnapshots = isRequiringNoSnapshots;
			this.IsDeletingOnlySnapshots = isDeletingOnlySnapshots;
			this.LeaseId = leaseId;
		}

		public override string ToString()
		{
			CultureInfo invariantCulture = CultureInfo.InvariantCulture;
			object[] includeDisabledContainers = new object[] { this.IncludeDisabledContainers, this.IncludeExpiredContainers, this.IncludeSnapshots, null, null, null, null, null, null };
			includeDisabledContainers[3] = (this.IfModifiedSinceTime.HasValue ? this.IfModifiedSinceTime.Value.ToString("O") : "<null>");
			includeDisabledContainers[4] = (this.IfNotModifiedSinceTime.HasValue ? this.IfNotModifiedSinceTime.Value.ToString("O") : "<null>");
			includeDisabledContainers[5] = (this.LeaseId.HasValue ? this.LeaseId.Value.ToString("N") : "<null>");
			includeDisabledContainers[6] = (this.SnapshotTimestamp.HasValue ? this.SnapshotTimestamp.Value.ToString("O") : "<null>");
			includeDisabledContainers[7] = (this.IsRequiringNoSnapshots.HasValue ? this.IsRequiringNoSnapshots.Value.ToString() : "<null>");
			includeDisabledContainers[8] = (this.IsDeletingOnlySnapshots.HasValue ? this.IsDeletingOnlySnapshots.Value.ToString() : "<null>");
			return string.Format(invariantCulture, "ContainerCondition(IncludeDisabledContainers={0}, IncludeExpiredContainers={1}, IncludeSnapshots={2}, IfModifiedSinceTime={3},IfNotModifiedSinceTime={4}, leaseId={5}, SnapshotTimestamp={6}, IsRequiringNoSnapshots={7}, IsDeletingOnlySnapshots={8})", includeDisabledContainers);
		}
	}
}