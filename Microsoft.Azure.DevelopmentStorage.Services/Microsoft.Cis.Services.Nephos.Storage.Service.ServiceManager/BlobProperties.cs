using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlobProperties : PutBlobProperties, IBlobProperties, IPutBlobProperties
	{
		private long contentLength;

		private long? sequenceNumber;

		private string copyId;

		private string copySource;

		private string copyStatus;

		private string copyStatusDescription;

		private string copyProgress;

		private DateTime? copyCompletionTime;

		private DateTime? lastCopySnapshot;

		public int? CommittedBlockCount
		{
			get
			{
				return JustDecompileGenerated_get_CommittedBlockCount();
			}
			set
			{
				JustDecompileGenerated_set_CommittedBlockCount(value);
			}
		}

		private int? JustDecompileGenerated_CommittedBlockCount_k__BackingField;

		public int? JustDecompileGenerated_get_CommittedBlockCount()
		{
			return this.JustDecompileGenerated_CommittedBlockCount_k__BackingField;
		}

		public void JustDecompileGenerated_set_CommittedBlockCount(int? value)
		{
			this.JustDecompileGenerated_CommittedBlockCount_k__BackingField = value;
		}

		public long ContentLength
		{
			get
			{
				return JustDecompileGenerated_get_ContentLength();
			}
			set
			{
				JustDecompileGenerated_set_ContentLength(value);
			}
		}

		public long JustDecompileGenerated_get_ContentLength()
		{
			return this.contentLength;
		}

		public void JustDecompileGenerated_set_ContentLength(long value)
		{
			this.contentLength = value;
		}

		public DateTime? CopyCompletionTime
		{
			get
			{
				return JustDecompileGenerated_get_CopyCompletionTime();
			}
			set
			{
				JustDecompileGenerated_set_CopyCompletionTime(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_CopyCompletionTime()
		{
			return this.copyCompletionTime;
		}

		public void JustDecompileGenerated_set_CopyCompletionTime(DateTime? value)
		{
			this.copyCompletionTime = value;
		}

		public string CopyId
		{
			get
			{
				return JustDecompileGenerated_get_CopyId();
			}
			set
			{
				JustDecompileGenerated_set_CopyId(value);
			}
		}

		public string JustDecompileGenerated_get_CopyId()
		{
			return this.copyId;
		}

		public void JustDecompileGenerated_set_CopyId(string value)
		{
			this.copyId = value;
		}

		public string CopyProgress
		{
			get
			{
				return JustDecompileGenerated_get_CopyProgress();
			}
			set
			{
				JustDecompileGenerated_set_CopyProgress(value);
			}
		}

		public string JustDecompileGenerated_get_CopyProgress()
		{
			return this.copyProgress;
		}

		public void JustDecompileGenerated_set_CopyProgress(string value)
		{
			this.copyProgress = value;
		}

		public string CopySource
		{
			get
			{
				return JustDecompileGenerated_get_CopySource();
			}
			set
			{
				JustDecompileGenerated_set_CopySource(value);
			}
		}

		public string JustDecompileGenerated_get_CopySource()
		{
			return this.copySource;
		}

		public void JustDecompileGenerated_set_CopySource(string value)
		{
			this.copySource = value;
		}

		public string CopyStatus
		{
			get
			{
				return JustDecompileGenerated_get_CopyStatus();
			}
			set
			{
				JustDecompileGenerated_set_CopyStatus(value);
			}
		}

		public string JustDecompileGenerated_get_CopyStatus()
		{
			return this.copyStatus;
		}

		public void JustDecompileGenerated_set_CopyStatus(string value)
		{
			this.copyStatus = value;
		}

		public string CopyStatusDescription
		{
			get
			{
				return JustDecompileGenerated_get_CopyStatusDescription();
			}
			set
			{
				JustDecompileGenerated_set_CopyStatusDescription(value);
			}
		}

		public string JustDecompileGenerated_get_CopyStatusDescription()
		{
			return this.copyStatusDescription;
		}

		public void JustDecompileGenerated_set_CopyStatusDescription(string value)
		{
			this.copyStatusDescription = value;
		}

		public string DiskResourceUri
		{
			get;
			set;
		}

		public string DiskTag
		{
			get;
			set;
		}

		public bool? IsBlobEncrypted
		{
			get
			{
				return JustDecompileGenerated_get_IsBlobEncrypted();
			}
			set
			{
				JustDecompileGenerated_set_IsBlobEncrypted(value);
			}
		}

		private bool? JustDecompileGenerated_IsBlobEncrypted_k__BackingField;

		public bool? JustDecompileGenerated_get_IsBlobEncrypted()
		{
			return this.JustDecompileGenerated_IsBlobEncrypted_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsBlobEncrypted(bool? value)
		{
			this.JustDecompileGenerated_IsBlobEncrypted_k__BackingField = value;
		}

		public bool IsBlobWriteProtected
		{
			get;
			set;
		}

		public bool IsIncrementalCopy
		{
			get
			{
				return JustDecompileGenerated_get_IsIncrementalCopy();
			}
			set
			{
				JustDecompileGenerated_set_IsIncrementalCopy(value);
			}
		}

		private bool JustDecompileGenerated_IsIncrementalCopy_k__BackingField;

		public bool JustDecompileGenerated_get_IsIncrementalCopy()
		{
			return this.JustDecompileGenerated_IsIncrementalCopy_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsIncrementalCopy(bool value)
		{
			this.JustDecompileGenerated_IsIncrementalCopy_k__BackingField = value;
		}

		public DateTime? LastCopySnapshot
		{
			get
			{
				return JustDecompileGenerated_get_LastCopySnapshot();
			}
			set
			{
				JustDecompileGenerated_set_LastCopySnapshot(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_LastCopySnapshot()
		{
			return this.lastCopySnapshot;
		}

		public void JustDecompileGenerated_set_LastCopySnapshot(DateTime? value)
		{
			this.lastCopySnapshot = value;
		}

		public long? SequenceNumber
		{
			get
			{
				return JustDecompileGenerated_get_SequenceNumber();
			}
			set
			{
				JustDecompileGenerated_set_SequenceNumber(value);
			}
		}

		public long? JustDecompileGenerated_get_SequenceNumber()
		{
			return this.sequenceNumber;
		}

		public void JustDecompileGenerated_set_SequenceNumber(long? value)
		{
			this.sequenceNumber = value;
		}

		public BlobProperties()
		{
		}
	}
}