using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class BlobObjectCondition : IBlobObjectCondition, IBaseObjectCondition
	{
		private bool includeDisabledContainers;

		private bool includeExpiredBlobs;

		private DateTime? ifModifiedSinceTime;

		private DateTime? ifNotModifiedSinceTime;

		private DateTime[] ifLastModificationTimeMatch;

		private DateTime[] ifLastModificationTimeMismatch;

		private IUpdateOptions updateOptions;

		private Guid? leaseId;

		private bool isFetchingMetadata;

		private bool isIncludingSnapshots;

		private bool isIncludingPageBlobs;

		private bool isIncludingAppendBlobs;

		private bool isIncludingUncommittedBlobs = true;

		private bool isDeletingOnlySnapshots;

		private bool isRequiringNoSnapshots;

		private bool isSkippingLastModificationTimeUpdate;

		private ComparisonOperator? sequenceNumberOperator;

		private long? sequenceNumber;

		private BlobType requiredBlobType;

		private bool isMultipleConditionalHeaderEnabled;

		private bool isCopyOperationOnAppendBlobsAllowed = true;

		private bool isOperationAllowedOnArchivedBlobs;

		private bool skipLeaseCheck;

		private DateTime? internalArchiveBlobLMT;

		private string internalArchiveBlobGenerationId;

		private Guid? internalArchiveRequestId;

		public bool EnsureBlobIsAlreadySoftDeleted
		{
			get;
			set;
		}

		public bool ExcludeSoftDeletedBlobs
		{
			get;
			set;
		}

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
			this.ifLastModificationTimeMatch = value;
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
			this.ifLastModificationTimeMismatch = value;
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

		public bool JustDecompileGenerated_get_IncludeDisabledContainers()
		{
			return this.includeDisabledContainers;
		}

		public void JustDecompileGenerated_set_IncludeDisabledContainers(bool value)
		{
			this.includeDisabledContainers = value;
		}

		public bool IncludeExpiredBlobs
		{
			get
			{
				return this.includeExpiredBlobs;
			}
			set
			{
				this.includeExpiredBlobs = value;
			}
		}

		public bool IncludeSoftDeletedBlobsOnly
		{
			get;
			set;
		}

		public string InternalArchiveBlobGenerationId
		{
			get
			{
				return JustDecompileGenerated_get_InternalArchiveBlobGenerationId();
			}
			set
			{
				JustDecompileGenerated_set_InternalArchiveBlobGenerationId(value);
			}
		}

		public string JustDecompileGenerated_get_InternalArchiveBlobGenerationId()
		{
			return this.internalArchiveBlobGenerationId;
		}

		public void JustDecompileGenerated_set_InternalArchiveBlobGenerationId(string value)
		{
			this.internalArchiveBlobGenerationId = value;
		}

		public DateTime? InternalArchiveBlobLMT
		{
			get
			{
				return JustDecompileGenerated_get_InternalArchiveBlobLMT();
			}
			set
			{
				JustDecompileGenerated_set_InternalArchiveBlobLMT(value);
			}
		}

		public DateTime? JustDecompileGenerated_get_InternalArchiveBlobLMT()
		{
			return this.internalArchiveBlobLMT;
		}

		public void JustDecompileGenerated_set_InternalArchiveBlobLMT(DateTime? value)
		{
			this.internalArchiveBlobLMT = value;
		}

		public Guid? InternalArchiveRequestId
		{
			get
			{
				return JustDecompileGenerated_get_InternalArchiveRequestId();
			}
			set
			{
				JustDecompileGenerated_set_InternalArchiveRequestId(value);
			}
		}

		public Guid? JustDecompileGenerated_get_InternalArchiveRequestId()
		{
			return this.internalArchiveRequestId;
		}

		public void JustDecompileGenerated_set_InternalArchiveRequestId(Guid? value)
		{
			this.internalArchiveRequestId = value;
		}

		public bool IsCopyOperationOnAppendBlobsAllowed
		{
			get
			{
				return JustDecompileGenerated_get_IsCopyOperationOnAppendBlobsAllowed();
			}
			set
			{
				JustDecompileGenerated_set_IsCopyOperationOnAppendBlobsAllowed(value);
			}
		}

		public bool JustDecompileGenerated_get_IsCopyOperationOnAppendBlobsAllowed()
		{
			return this.isCopyOperationOnAppendBlobsAllowed;
		}

		public void JustDecompileGenerated_set_IsCopyOperationOnAppendBlobsAllowed(bool value)
		{
			this.isCopyOperationOnAppendBlobsAllowed = value;
		}

		public bool IsDeletingOnlySnapshots
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

		public bool JustDecompileGenerated_get_IsDeletingOnlySnapshots()
		{
			return this.isDeletingOnlySnapshots;
		}

		public void JustDecompileGenerated_set_IsDeletingOnlySnapshots(bool value)
		{
			this.isDeletingOnlySnapshots = value;
		}

		public bool IsDiskMountStateConditionRequired
		{
			get;
			set;
		}

		public bool IsFetchingMetadata
		{
			get
			{
				return JustDecompileGenerated_get_IsFetchingMetadata();
			}
			set
			{
				JustDecompileGenerated_set_IsFetchingMetadata(value);
			}
		}

		public bool JustDecompileGenerated_get_IsFetchingMetadata()
		{
			return this.isFetchingMetadata;
		}

		public void JustDecompileGenerated_set_IsFetchingMetadata(bool value)
		{
			this.isFetchingMetadata = value;
		}

		public bool IsIncludingAppendBlobs
		{
			get
			{
				return JustDecompileGenerated_get_IsIncludingAppendBlobs();
			}
			set
			{
				JustDecompileGenerated_set_IsIncludingAppendBlobs(value);
			}
		}

		public bool JustDecompileGenerated_get_IsIncludingAppendBlobs()
		{
			return this.isIncludingAppendBlobs;
		}

		public void JustDecompileGenerated_set_IsIncludingAppendBlobs(bool value)
		{
			this.isIncludingAppendBlobs = value;
		}

		public bool IsIncludingPageBlobs
		{
			get
			{
				return JustDecompileGenerated_get_IsIncludingPageBlobs();
			}
			set
			{
				JustDecompileGenerated_set_IsIncludingPageBlobs(value);
			}
		}

		public bool JustDecompileGenerated_get_IsIncludingPageBlobs()
		{
			return this.isIncludingPageBlobs;
		}

		public void JustDecompileGenerated_set_IsIncludingPageBlobs(bool value)
		{
			this.isIncludingPageBlobs = value;
		}

		public bool IsIncludingSnapshots
		{
			get
			{
				return JustDecompileGenerated_get_IsIncludingSnapshots();
			}
			set
			{
				JustDecompileGenerated_set_IsIncludingSnapshots(value);
			}
		}

		public bool JustDecompileGenerated_get_IsIncludingSnapshots()
		{
			return this.isIncludingSnapshots;
		}

		public void JustDecompileGenerated_set_IsIncludingSnapshots(bool value)
		{
			this.isIncludingSnapshots = value;
		}

		public bool IsIncludingUncommittedBlobs
		{
			get
			{
				return JustDecompileGenerated_get_IsIncludingUncommittedBlobs();
			}
			set
			{
				JustDecompileGenerated_set_IsIncludingUncommittedBlobs(value);
			}
		}

		public bool JustDecompileGenerated_get_IsIncludingUncommittedBlobs()
		{
			return this.isIncludingUncommittedBlobs;
		}

		public void JustDecompileGenerated_set_IsIncludingUncommittedBlobs(bool value)
		{
			this.isIncludingUncommittedBlobs = value;
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

		public bool IsOperationAllowedOnArchivedBlobs
		{
			get
			{
				return this.isOperationAllowedOnArchivedBlobs;
			}
			set
			{
				this.isOperationAllowedOnArchivedBlobs = value;
			}
		}

		public DateTime? IsRequiringLeaseIfNotModifiedSince
		{
			get
			{
				return JustDecompileGenerated_get_IsRequiringLeaseIfNotModifiedSince();
			}
			set
			{
				JustDecompileGenerated_set_IsRequiringLeaseIfNotModifiedSince(value);
			}
		}

		private DateTime? JustDecompileGenerated_IsRequiringLeaseIfNotModifiedSince_k__BackingField;

		public DateTime? JustDecompileGenerated_get_IsRequiringLeaseIfNotModifiedSince()
		{
			return this.JustDecompileGenerated_IsRequiringLeaseIfNotModifiedSince_k__BackingField;
		}

		public void JustDecompileGenerated_set_IsRequiringLeaseIfNotModifiedSince(DateTime? value)
		{
			this.JustDecompileGenerated_IsRequiringLeaseIfNotModifiedSince_k__BackingField = value;
		}

		public bool IsRequiringNoSnapshots
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

		public bool JustDecompileGenerated_get_IsRequiringNoSnapshots()
		{
			return this.isRequiringNoSnapshots;
		}

		public void JustDecompileGenerated_set_IsRequiringNoSnapshots(bool value)
		{
			this.isRequiringNoSnapshots = value;
		}

		public bool IsSkippingLastModificationTimeUpdate
		{
			get
			{
				return JustDecompileGenerated_get_IsSkippingLastModificationTimeUpdate();
			}
			set
			{
				JustDecompileGenerated_set_IsSkippingLastModificationTimeUpdate(value);
			}
		}

		public bool JustDecompileGenerated_get_IsSkippingLastModificationTimeUpdate()
		{
			return this.isSkippingLastModificationTimeUpdate;
		}

		public void JustDecompileGenerated_set_IsSkippingLastModificationTimeUpdate(bool value)
		{
			this.isSkippingLastModificationTimeUpdate = value;
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

		public Guid? JustDecompileGenerated_get_LeaseId()
		{
			return this.leaseId;
		}

		public void JustDecompileGenerated_set_LeaseId(Guid? value)
		{
			this.leaseId = value;
		}

		public BlobType RequiredBlobType
		{
			get
			{
				return JustDecompileGenerated_get_RequiredBlobType();
			}
			set
			{
				JustDecompileGenerated_set_RequiredBlobType(value);
			}
		}

		public BlobType JustDecompileGenerated_get_RequiredBlobType()
		{
			return this.requiredBlobType;
		}

		public void JustDecompileGenerated_set_RequiredBlobType(BlobType value)
		{
			this.requiredBlobType = value;
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

		public ComparisonOperator? SequenceNumberOperator
		{
			get
			{
				return JustDecompileGenerated_get_SequenceNumberOperator();
			}
			set
			{
				JustDecompileGenerated_set_SequenceNumberOperator(value);
			}
		}

		public ComparisonOperator? JustDecompileGenerated_get_SequenceNumberOperator()
		{
			return this.sequenceNumberOperator;
		}

		public void JustDecompileGenerated_set_SequenceNumberOperator(ComparisonOperator? value)
		{
			this.sequenceNumberOperator = value;
		}

		public bool SkipLeaseCheck
		{
			get
			{
				return this.skipLeaseCheck;
			}
			set
			{
				this.skipLeaseCheck = value;
			}
		}

		public IUpdateOptions UpdateOptions
		{
			get
			{
				return JustDecompileGenerated_get_UpdateOptions();
			}
			set
			{
				JustDecompileGenerated_set_UpdateOptions(value);
			}
		}

		public IUpdateOptions JustDecompileGenerated_get_UpdateOptions()
		{
			return this.updateOptions;
		}

		public void JustDecompileGenerated_set_UpdateOptions(IUpdateOptions value)
		{
			this.updateOptions = value;
		}

		public BlobObjectCondition()
		{
			this.ExcludeSoftDeletedBlobs = true;
			this.EnsureBlobIsAlreadySoftDeleted = false;
			this.IncludeSoftDeletedBlobsOnly = false;
			this.isOperationAllowedOnArchivedBlobs = false;
			this.skipLeaseCheck = false;
		}

		public BlobObjectCondition(DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, Guid? leaseId) : this()
		{
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.leaseId = leaseId;
		}

		public BlobObjectCondition(bool isOperationAllowedOnArchivedBlobs, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, Guid? leaseId, bool skipLeaseCheck, DateTime? internalArchiveBlobLMT, string internalArchiveBlobGenerationId) : this()
		{
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.leaseId = leaseId;
			this.isOperationAllowedOnArchivedBlobs = isOperationAllowedOnArchivedBlobs;
			this.skipLeaseCheck = skipLeaseCheck;
			this.internalArchiveBlobLMT = internalArchiveBlobLMT;
			this.internalArchiveBlobGenerationId = internalArchiveBlobGenerationId;
		}

		public BlobObjectCondition(bool includeDisabledContainers, bool includeExpiredBlobs, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime) : this()
		{
			this.includeDisabledContainers = includeDisabledContainers;
			this.includeExpiredBlobs = includeExpiredBlobs;
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
		}

		public BlobObjectCondition(bool includeDisabledContainers, bool includeExpiredBlobs, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, Guid? leaseId) : this()
		{
			this.includeDisabledContainers = includeDisabledContainers;
			this.includeExpiredBlobs = includeExpiredBlobs;
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.leaseId = leaseId;
		}

		public BlobObjectCondition(bool includeDisabledContainers, bool includeExpiredBlobs, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, DateTime[] ifLastModificationTimeMatch, DateTime[] ifLastModificationTimeMismatch, IUpdateOptions updateOptions, Guid? leaseId, bool isFetchingMetadata, bool isIncludingSnapshots, bool isIncludingPageBlobs, bool isIncludingAppendBlobs, bool isIncludingUncommittedBlobs, bool isDeletingOnlySnapshots, bool isRequiringNoSnapshots, bool isSkippingLastModificationTimeUpdate, ComparisonOperator? sequenceNumberOperator, long? sequenceNumber, BlobType requiredBlobType, bool isMultipleConditionalHeaderEnabled, bool isOperationOnAppendBlobsAllowed, DateTime? isRequiringLeaseIfNotModifiedSince, bool isDiskMountStateConditionRequired, bool excludeSoftDeletedBlobs, bool ensureBlobIsAlreadySoftDeleted, bool isOperationAllowedOnArchivedBlobs, bool skipLeaseCheck, DateTime? internalArchiveBlobLMT, string internalArchiveBlobGenerationId, Guid? internalArchiveRequestId, bool includeSoftDeletedBlobsOnly)
		{
			if (sequenceNumberOperator.HasValue && !sequenceNumber.HasValue || !sequenceNumberOperator.HasValue && sequenceNumber.HasValue)
			{
				throw new ArgumentException(string.Format("sequenceNumberOperator and sequenceNumber must both be set in order to apply a sequence number condition. sequenceNumberOperator = {0}, sequenceNumber = {1}.", (sequenceNumberOperator.HasValue ? sequenceNumberOperator.Value.ToString() : "<null>"), (sequenceNumber.HasValue ? sequenceNumber.Value.ToString() : "<null>")));
			}
			this.includeDisabledContainers = includeDisabledContainers;
			this.includeExpiredBlobs = includeExpiredBlobs;
			this.ifModifiedSinceTime = ifModifiedSinceTime;
			this.ifNotModifiedSinceTime = ifNotModifiedSinceTime;
			this.ifLastModificationTimeMatch = ifLastModificationTimeMatch;
			this.ifLastModificationTimeMismatch = ifLastModificationTimeMismatch;
			this.updateOptions = updateOptions;
			this.leaseId = leaseId;
			this.isFetchingMetadata = isFetchingMetadata;
			this.isIncludingSnapshots = isIncludingSnapshots;
			this.isIncludingPageBlobs = isIncludingPageBlobs;
			this.isIncludingAppendBlobs = isIncludingAppendBlobs;
			this.isIncludingUncommittedBlobs = isIncludingUncommittedBlobs;
			this.isDeletingOnlySnapshots = isDeletingOnlySnapshots;
			this.isRequiringNoSnapshots = isRequiringNoSnapshots;
			this.isSkippingLastModificationTimeUpdate = isSkippingLastModificationTimeUpdate;
			this.sequenceNumberOperator = sequenceNumberOperator;
			this.sequenceNumber = sequenceNumber;
			this.requiredBlobType = requiredBlobType;
			this.IsMultipleConditionalHeaderEnabled = isMultipleConditionalHeaderEnabled;
			this.IsCopyOperationOnAppendBlobsAllowed = isOperationOnAppendBlobsAllowed;
			this.IsRequiringLeaseIfNotModifiedSince = isRequiringLeaseIfNotModifiedSince;
			this.IsDiskMountStateConditionRequired = isDiskMountStateConditionRequired;
			this.IncludeSoftDeletedBlobsOnly = includeSoftDeletedBlobsOnly;
			this.ExcludeSoftDeletedBlobs = excludeSoftDeletedBlobs;
			this.EnsureBlobIsAlreadySoftDeleted = ensureBlobIsAlreadySoftDeleted;
			this.isOperationAllowedOnArchivedBlobs = isOperationAllowedOnArchivedBlobs;
			this.skipLeaseCheck = skipLeaseCheck;
			this.internalArchiveBlobLMT = internalArchiveBlobLMT;
			this.internalArchiveBlobGenerationId = internalArchiveBlobGenerationId;
			this.internalArchiveRequestId = internalArchiveRequestId;
		}

		public override string ToString()
		{
			object[] objArray = new object[] { this.includeDisabledContainers, this.includeExpiredBlobs, (this.ifModifiedSinceTime.HasValue ? this.ifModifiedSinceTime.Value.ToString("O") : "<null>"), (this.ifNotModifiedSinceTime.HasValue ? this.ifNotModifiedSinceTime.Value.ToString("O") : "<null>"), (this.leaseId.HasValue ? this.leaseId.Value.ToString("N") : "<null>"), this.isIncludingUncommittedBlobs, this.isFetchingMetadata, this.isIncludingSnapshots, this.isIncludingPageBlobs, this.isIncludingAppendBlobs, this.isDeletingOnlySnapshots, (this.ifLastModificationTimeMatch != null ? StorageStampHelpers.DateTimeArrayToString(this.ifLastModificationTimeMatch) : "<null>"), (this.ifLastModificationTimeMismatch != null ? StorageStampHelpers.DateTimeArrayToString(this.ifLastModificationTimeMismatch) : "<null>"), this.isRequiringNoSnapshots, this.isSkippingLastModificationTimeUpdate, (this.sequenceNumberOperator.HasValue ? this.sequenceNumberOperator.Value.ToString() : "<null>"), (this.sequenceNumber.HasValue ? this.sequenceNumber.Value.ToString() : "<null>"), this.requiredBlobType, this.IsMultipleConditionalHeaderEnabled, this.IsCopyOperationOnAppendBlobsAllowed, (this.IsRequiringLeaseIfNotModifiedSince.HasValue ? this.IsRequiringLeaseIfNotModifiedSince.Value.ToString("O") : "<null>"), this.IsDiskMountStateConditionRequired, this.ExcludeSoftDeletedBlobs, this.EnsureBlobIsAlreadySoftDeleted, this.isOperationAllowedOnArchivedBlobs, this.skipLeaseCheck, (this.internalArchiveBlobLMT.HasValue ? this.internalArchiveBlobLMT.Value.ToString("O") : "<null>"), (!string.IsNullOrEmpty(this.internalArchiveBlobGenerationId) ? this.internalArchiveBlobGenerationId : "<null>"), (this.internalArchiveRequestId.HasValue ? this.internalArchiveRequestId.ToString() : "<null>"), this.IncludeSoftDeletedBlobsOnly };
			return string.Format("[includeDisabledContainers = {0}, includeExpiredBlobs = {1}, ifModifiedSinceTime = {2}, ifNotModifiedSinceTime = {3}, leaseId = {4}, isIncludingUncommittedBlobs = {5}, isFetchingMetadata = {6}, isIncludingSnapshots = {7}, isIncludingPageBlobs = {8} isIncludingAppendBlobs = {9}, isDeletingOnlySnapshots = {10}, ifLastModificationTimeMatch = {11}, ifLastModificationTimeMismatch = {12}, isRequiringNoSnapshots = {13}, isSkippingLastModificationTimeUpdate = {14}, sequenceNumberOperator = {15}, sequenceNumber = {16}, requiredBlobType = {17}, isMultipleConditionalHeaderEnabled = {18}, IsOperationOnAppendBlobsAllowed = {19}, IsRequiringLeaseIfNotModifiedSince = {20}, isDiskMountStateConditionRequired = {21}, ExcludeSoftDeletedBlobs = {22}, EnsureBlobIsAlreadySoftdeleted = {23}, isOperationAllowedOnArchivedBlobs = {24}, skipLeaseCheck = {25}, internalArchiveBlobLMT = {26}, internalArchiveBlobGenerationId = {27}, internalArchvieRequestId = {28}, includeSoftDeletedBlobsOnly = {29}]", objArray);
		}
	}
}