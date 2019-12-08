using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class FileObjectCondition : IFileObjectCondition, IBaseObjectCondition
	{
		private DateTime? ifModifiedSinceTime;

		private DateTime? ifNotModifiedSinceTime;

		private DateTime[] ifLastModificationTimeMatch;

		private DateTime[] ifLastModificationTimeMismatch;

		private bool isMultipleConditionalHeaderEnabled;

		public DateTime[] IfLastModificationTimeMatch
		{
			get
			{
				return JustDecompileGenerated_get_IfLastModificationTimeMatch();
			}
			set
			{
				JustDecompileGenerated_set_IfLastModificationTimeMatch(value);
			}
		}

		public DateTime[] JustDecompileGenerated_get_IfLastModificationTimeMatch()
		{
			return this.ifLastModificationTimeMatch;
		}

		public void JustDecompileGenerated_set_IfLastModificationTimeMatch(DateTime[] value)
		{
			this.ifLastModificationTimeMatch = StorageStampHelpers.AdjustDateTimeRange(value);
		}

		public DateTime[] IfLastModificationTimeMismatch
		{
			get
			{
				return JustDecompileGenerated_get_IfLastModificationTimeMismatch();
			}
			set
			{
				JustDecompileGenerated_set_IfLastModificationTimeMismatch(value);
			}
		}

		public DateTime[] JustDecompileGenerated_get_IfLastModificationTimeMismatch()
		{
			return this.ifLastModificationTimeMismatch;
		}

		public void JustDecompileGenerated_set_IfLastModificationTimeMismatch(DateTime[] value)
		{
			this.ifLastModificationTimeMismatch = StorageStampHelpers.AdjustDateTimeRange(value);
		}

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
			this.ifModifiedSinceTime = StorageStampHelpers.AdjustNullableDatetimeRange(value);
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
			this.ifNotModifiedSinceTime = StorageStampHelpers.AdjustNullableDatetimeRange(value);
		}

		public bool IsMultipleConditionalHeaderEnabled
		{
			get
			{
				return this.isMultipleConditionalHeaderEnabled;
			}
			set
			{
				this.isMultipleConditionalHeaderEnabled = value;
			}
		}

		public FileObjectCondition()
		{
		}

		public FileObjectCondition(DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, DateTime[] ifLastModificationTimeMatch, DateTime[] ifLastModificationTimeMismatch)
		{
			this.IfModifiedSinceTime = ifModifiedSinceTime;
			this.IfNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.IfLastModificationTimeMatch = ifLastModificationTimeMatch;
			this.IfLastModificationTimeMismatch = ifLastModificationTimeMismatch;
		}

		public FileObjectCondition(DateTime[] ifLastModificationTimeMatch)
		{
			this.IfLastModificationTimeMatch = ifLastModificationTimeMatch;
		}

		public override string ToString()
		{
			object[] objArray = new object[] { (this.IfModifiedSinceTime.HasValue ? this.IfModifiedSinceTime.Value.ToString("O") : "<null>"), (this.IfNotModifiedSinceTime.HasValue ? this.IfNotModifiedSinceTime.Value.ToString("O") : "<null>"), (this.ifLastModificationTimeMatch != null ? StorageStampHelpers.DateTimeArrayToString(this.ifLastModificationTimeMatch) : "<null>"), (this.ifLastModificationTimeMismatch != null ? StorageStampHelpers.DateTimeArrayToString(this.ifLastModificationTimeMismatch) : "<null>"), this.IsMultipleConditionalHeaderEnabled };
			return string.Format("[ifModifiedSinceTime = {0}, ifNotModifiedSinceTime = {1}, ifLastModificationTimeMatch = {2}, ifLastModificationTimeMismatch = {3}, isMultipleConditionalHeaderEnabled = {4}]", objArray);
		}
	}
}