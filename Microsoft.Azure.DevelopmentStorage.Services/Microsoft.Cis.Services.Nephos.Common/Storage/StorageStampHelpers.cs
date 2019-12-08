using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public static class StorageStampHelpers
	{
		private const int ACCOUNT_NAME_MIN_LENGTH = 3;

		private const int ACCOUNT_NAME_MAX_LENGTH = 24;

		private const int CONTAINER_NAME_MIN_LENGTH = 3;

		private const int CONTAINER_NAME_MAX_LENGTH = 63;

		public const int BLOB_NAME_MAX_LENGTH = 1024;

		private const int DOMAIN_LABEL_MIN_LENGTH = 1;

		public const int DOMAIN_LABEL_MAX_LENGTH = 63;

		private const int MAX_STORAGE_DOMAIN_NAME_LENGTH_LIMIT = 255;

		public const int MinListingResults = 1;

		public const int MaxListingResults = 5000;

		public const BlobServiceVersion UnspecifiedBlobServiceVersion = BlobServiceVersion.Original;

		public const int XBLOB_MAX_BLOCK_ID_LENGTH = 64;

		public const int XBLOB_FLAG_COOL_BLOB = 128;

		public const int XBLOB_FLAG_HOT_BLOB = 65536;

		public const int XBLOB_FLAG_ARCHIVED_BLOB = 131072;

		public const int XBLOB_FLAG_ARCHIVE_PENDING_BLOB = 262144;

		public const int XBLOB_FLAG_REHYDRATE_PENDING_BLOB = 524288;

		public const int XBLOB_MAX_APPLICATION_METADATA_SIZE = 8192;

		public const int XBLOB_MAX_SERVICE_METADATA_SIZE = 8192;

		public const long MEGABYTE = 1048576L;

		public const long GIGABYTE = 1073741824L;

		public const long TERABYTE = 1099511627776L;

		public const long XBLOB_MAX_PUT_BLOCK_SIZE = 4194304L;

		public const long XBLOB_MAX_LARGE_PUT_BLOCK_SIZE = 104857600L;

		public const long XBLOB_MAX_APPEND_BLOCK_SIZE = 4194304L;

		public const int XBLOB_MAX_APPEND_BLOCK_COUNT = 50000;

		public const long XBLOB_MAX_APPEND_BLOCK_BLOB_SIZE = 209715200000L;

		public const long XBLOB_MAX_PUT_PAGE_STREAM_SIZE = 4194304L;

		public const long XBLOB_MAX_PUT_BLOB_STREAM_SIZE = 67108864L;

		public const long XBLOB_MAX_LARGE_PUT_BLOB_STREAM_SIZE = 268435456L;

		public const long XBLOB_MAX_GET_BLOB_SIZE = 4194304L;

		public const long XBLOB_MIN_INDEX_BLOB_SIZE = 0L;

		public const long XBLOB_MAX_INDEX_BLOB_SIZE = 1099511627776L;

		public const long XBLOB_MAX_FILE_SIZE = 1099511627776L;

		public const long XBLOB_MAX_INDEX_BLOB_SIZE_8TB = 8796093022208L;

		public const long XBLOB_MAX_BLOCK_LIST_LENGTH = 50000L;

		public const long XBLOB_MAX_UNCOMMITTED_BLOCK_LIST_LENGTH = 100000L;

		public const long XBLOB_MAX_NON_LARGE_PUT_BLOB_BLOCK_LIST_SIZE = 209715200000L;

		public const long XBLOB_MAX_PUT_BLOB_BLOCK_LIST_SIZE = 5242880000000L;

		public const long XBLOB_MAX_UNCOMMITTED_BLOCK_LIST_SIZE = 10485760000000L;

		public const long XBLOB_MAX_PUT_PAGE_SIZE = 67108864L;

		public const long XBLOB_MD5_LENGTH = 16L;

		public const int XPAGE_PAGE_SIZE = 512;

		private readonly static TimeSpan maximumAllowedXStoreTimeout;

		private readonly static DateTime minimumAllowedXStoreDateTimeValue;

		private readonly static Regex ALPHA_NUMERIC_REGEX;

		private static string STORAGE_DOMAIN_NAME_LABEL_SEPARATOR_CHAR;

		private static string DOMAIN_LABEL_ALLOWED_PATTERN;

		public readonly static DateTime RootBlobSnapshotVersion;

		public static int MaxAccountNameLength
		{
			get
			{
				return 24;
			}
		}

		public static int MaxContainerNameLength
		{
			get
			{
				return 63;
			}
		}

		public static int MaxServicePropertiesLength
		{
			get
			{
				return 8192;
			}
		}

		static StorageStampHelpers()
		{
			StorageStampHelpers.maximumAllowedXStoreTimeout = TimeSpan.FromMilliseconds(2147483647);
			StorageStampHelpers.minimumAllowedXStoreDateTimeValue = DateTime.FromFileTimeUtc((long)0);
			StorageStampHelpers.ALPHA_NUMERIC_REGEX = new Regex("^[a-zA-Z0-9-]+$");
			StorageStampHelpers.STORAGE_DOMAIN_NAME_LABEL_SEPARATOR_CHAR = ".";
			StorageStampHelpers.DOMAIN_LABEL_ALLOWED_PATTERN = "^[A-Za-z0-9]+(-[A-Za-z0-9]+)*";
			StorageStampHelpers.RootBlobSnapshotVersion = DateTime.MaxValue;
		}

		public static DateTime AdjustDateTimeRange(DateTime datetime)
		{
			if (datetime >= StorageStampHelpers.minimumAllowedXStoreDateTimeValue)
			{
				return datetime;
			}
			return StorageStampHelpers.minimumAllowedXStoreDateTimeValue;
		}

		public static DateTime[] AdjustDateTimeRange(DateTime[] dateTime)
		{
			List<DateTime> dateTimes = new List<DateTime>();
			if (dateTime != null)
			{
				dateTimes.AddRange(
					from d in (IEnumerable<DateTime>)dateTime
					where true
					select StorageStampHelpers.AdjustDateTimeRange(d));
			}
			if (dateTimes.Count <= 0)
			{
				return null;
			}
			return dateTimes.ToArray();
		}

		public static DateTime? AdjustNullableDatetimeRange(DateTime? datetime)
		{
			if (!datetime.HasValue)
			{
				return null;
			}
			return new DateTime?(StorageStampHelpers.AdjustDateTimeRange(datetime.Value));
		}

		public static TimeSpan AdjustTimeoutRange(TimeSpan timeout)
		{
			if (timeout <= StorageStampHelpers.maximumAllowedXStoreTimeout)
			{
				return timeout;
			}
			return StorageStampHelpers.maximumAllowedXStoreTimeout;
		}

		public static void CheckAccountName(string accountName)
		{
			StorageStampHelpers.CheckAccountName(accountName, false);
		}

		public static void CheckAccountName(string accountName, bool isForSummary)
		{
			StorageStampHelpers.CheckAccountName(accountName, isForSummary, true);
		}

		public static bool CheckAccountName(string accountName, bool isForSummary, bool throwExceptionIfAccountNameIsInvalid)
		{
			if (string.IsNullOrEmpty(accountName))
			{
				if (isForSummary)
				{
					return true;
				}
				if (throwExceptionIfAccountNameIsInvalid)
				{
					throw new ArgumentNullException("accountName");
				}
				return false;
			}
			if (accountName.Length > 24 || accountName.Length < 3)
			{
				if (isForSummary)
				{
					return true;
				}
				if (throwExceptionIfAccountNameIsInvalid)
				{
					throw new ArgumentOutOfRangeException("accountName", string.Format("accountName must be at least {0} characters and at most {1} characters", 3, 24));
				}
				return false;
			}
			int accountNameInvalidChar = StorageStampHelpers.GetAccountNameInvalidChar(accountName);
			if (accountNameInvalidChar < 0)
			{
				return true;
			}
			if (isForSummary)
			{
				return true;
			}
			if (throwExceptionIfAccountNameIsInvalid)
			{
				throw new ArgumentOutOfRangeException("accountName", string.Format("The character '{0}' at index {1} is not allowed in the account name", StorageStampHelpers.MakeXmlFriendlyString(accountName[accountNameInvalidChar]), accountNameInvalidChar));
			}
			return false;
		}

		public static void CheckBlobName(string blobName, string containerName)
		{
			if (string.IsNullOrEmpty(blobName))
			{
				throw new ArgumentNullException("blobName");
			}
			if (containerName == null)
			{
				throw new ArgumentNullException("containerName");
			}
			if (blobName.Length > 1024)
			{
				throw new XStoreArgumentOutOfRangeException("blobName", string.Format("blobName must be at most {0} characters.", 1024));
			}
			if (containerName == "$root" && blobName.Contains("\\"))
			{
				throw new XStoreArgumentException("Blobs under the root container cannot have forward slashes.");
			}
		}

		public static void CheckContainerName(string containerName, ContainerType containerType, bool isSummaryContainer)
		{
			int num;
			if (string.IsNullOrEmpty(containerName))
			{
				if (!isSummaryContainer)
				{
					throw new ArgumentNullException("containerName");
				}
				return;
			}
			if (containerName.Length > 63 || containerName.Length < 3)
			{
				if (!isSummaryContainer)
				{
					throw new XStoreArgumentOutOfRangeException("containerName", string.Format("containerName must be at least {0} characters and at most {1} characters", 3, 63), "The specified resource name length is not within the permissible limits.");
				}
				return;
			}
			if ((containerType == ContainerType.BlobContainer || containerType == ContainerType.FileContainer) && SpecialNames.IsBlobContainerSpecialName(containerName))
			{
				return;
			}
			if (containerType == ContainerType.TableContainer && SpecialNames.IsTableContainerSpecialName(containerName))
			{
				return;
			}
			num = (containerType != ContainerType.TableContainer ? StorageStampHelpers.GetContainerNameInvalidChar(containerName) : StorageStampHelpers.GetTableNameInvalidChar(containerName));
			if (num < 0)
			{
				return;
			}
			if (!isSummaryContainer)
			{
				throw new InvalidResourceNameException(string.Format("The character '{0}' at index {1} is not allowed in the container name", StorageStampHelpers.MakeXmlFriendlyString(containerName[num]), num));
			}
		}

		public static void CheckStorageDomainName(string storageDomainName)
		{
			if (string.IsNullOrEmpty(storageDomainName))
			{
				throw new ArgumentNullException("storageDomainName");
			}
			if (storageDomainName.Length > 255)
			{
				throw new ArgumentOutOfRangeException("storageDomainName", string.Concat("Storage domain name exceeds the maximum limit of ", 255));
			}
			char[] sTORAGEDOMAINNAMELABELSEPARATORCHAR = new char[] { StorageStampHelpers.STORAGE_DOMAIN_NAME_LABEL_SEPARATOR_CHAR[0] };
			string[] strArrays = storageDomainName.Split(sTORAGEDOMAINNAMELABELSEPARATORCHAR);
			if ((int)strArrays.Length < 2)
			{
				throw new ArgumentException(string.Format("Storage domain {0} should have a valid top level domain and a label", storageDomainName), "storageDomainName");
			}
			string[] strArrays1 = strArrays;
			for (int i = 0; i < (int)strArrays1.Length; i++)
			{
				StorageStampHelpers.ValidateStorageDomainLabel(strArrays1[i]);
			}
		}

		public static void CheckSubscription(string accountName, string subscription)
		{
			if (!StorageStampHelpers.ALPHA_NUMERIC_REGEX.IsMatch(subscription.Trim()))
			{
				throw new XStoreArgumentException(string.Format("subscription for account {0} must be composed of alphanumeric characters or dashes", accountName));
			}
		}

		public static string DateTimeArrayToString(DateTime[] dateTimes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (dateTimes != null)
			{
				DateTime[] dateTimeArray = dateTimes;
				for (int i = 0; i < (int)dateTimeArray.Length; i++)
				{
					DateTime dateTime = dateTimeArray[i];
					stringBuilder.Append(dateTime.ToString("O"));
					stringBuilder.Append(",");
				}
			}
			return stringBuilder.ToString().TrimEnd(new char[] { ',' });
		}

		public static int GetAccountNameInvalidChar(string accountName)
		{
			for (int i = 0; i < accountName.Length; i++)
			{
				char chr = accountName[i];
				if ((chr < 'a' || chr > 'z') && (chr < '0' || chr > '9') && chr != '-')
				{
					return i;
				}
			}
			return -1;
		}

		public static int GetContainerNameInvalidChar(string containerName)
		{
			for (int i = 0; i < containerName.Length; i++)
			{
				char chr = containerName[i];
				if ((chr < 'a' || chr > 'z') && (chr < '0' || chr > '9') && (chr != '-' || i == 0 || i == containerName.Length - 1 || containerName[i - 1] == '-'))
				{
					return i;
				}
			}
			return -1;
		}

		private static int GetInvalidCharInName(string name, string allowedPattern, bool isBlobQueueContainerName)
		{
			if (string.IsNullOrEmpty(name))
			{
				throw new ArgumentNullException(name);
			}
			Match match = (new Regex(allowedPattern)).Match(name);
			if (match.Length == name.Length)
			{
				return -1;
			}
			if (!isBlobQueueContainerName || name[match.Length] != '-' || match.Length == 0 || match.Length == name.Length - 1)
			{
				return match.Length;
			}
			return match.Length + 1;
		}

		public static int GetTableNameInvalidChar(string tableName)
		{
			if (tableName.Length > 0)
			{
				char chr = tableName[0];
				if ((chr < 'a' || chr > 'z') && (chr < 'A' || chr > 'Z'))
				{
					return 0;
				}
			}
			for (int i = 1; i < tableName.Length; i++)
			{
				char chr1 = tableName[i];
				if ((chr1 < 'a' || chr1 > 'z') && (chr1 < '0' || chr1 > '9') && (chr1 < 'A' || chr1 > 'Z'))
				{
					return i;
				}
			}
			return -1;
		}

		private static string MakeXmlFriendlyString(char c)
		{
			if (c >= ' ' && c <= '\uD7FF' || c >= '\uE000' && c <= '\uFFFD' || c == '\t' || c == '\n' || c == '\r')
			{
				return new string(c, 1);
			}
			return c.ToString("x4");
		}

		public static void ValidateAppendBlockArguments(IListBlobObject blob, long contentLength, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition)
		{
			if (blob.Snapshot != StorageStampHelpers.RootBlobSnapshotVersion)
			{
				throw new XStoreArgumentException("This operation is only supported on the root blob.");
			}
			if (contentLength <= (long)0)
			{
				throw new ArgumentOutOfRangeException("contentLength", "contentLength must be > 0");
			}
			if (contentLength > (long)4194304)
			{
				throw new BlobContentTooLargeException(new long?((long)4194304), "The block being appended exceeds the maximum block size.", null);
			}
			if (conditionalMaxBlobSize.HasValue && conditionalMaxBlobSize.Value <= (long)0)
			{
				throw new XStoreArgumentOutOfRangeException("maxBlobSize", "maxBlobSize must be > 0.");
			}
			if (conditionalAppendBlockPosition.HasValue)
			{
				long value = conditionalAppendBlockPosition.Value;
				if (value < (long)0 || value >= 209715200000L)
				{
					throw new XStoreArgumentOutOfRangeException("appendPos", "appendPos must be >= 0 and < XBLOB_MAX_APPEND_BLOCK_BLOB_SIZE");
				}
			}
		}

		public static void ValidateApplicationMetadata(NameValueCollection metadata)
		{
			StorageStampHelpers.ValidateApplicationMetadata(MetadataEncoding.GetMetadataLengthWithAsciiEncoding(metadata));
		}

		public static void ValidateApplicationMetadata(byte[] applicationMetadata)
		{
			if (applicationMetadata != null)
			{
				StorageStampHelpers.ValidateApplicationMetadata((int)applicationMetadata.Length);
			}
		}

		private static void ValidateApplicationMetadata(int metadataSize)
		{
			if (metadataSize > 8192)
			{
				int num = 8192;
				throw new ObjectMetadataOverLimitException(string.Concat("The metadata should be at most ", num.ToString(), " bytes."));
			}
		}

		private static void ValidateMaxBlobSizeForPutBlob(IBlobObject blob, long? maxBlobSize)
		{
			if (blob.Type == BlobType.ListBlob)
			{
				if (maxBlobSize.HasValue)
				{
					throw new XStoreArgumentException("The maximum blob size is not valid for a BlockBlob.");
				}
				return;
			}
			if (blob.Type == BlobType.IndexBlob)
			{
				if (!maxBlobSize.HasValue)
				{
					throw new XStoreArgumentException("The maximum blob size must be set for a PageBlob.");
				}
				if (maxBlobSize.Value < (long)0 || maxBlobSize.Value > 8796093022208L)
				{
					throw new XStoreArgumentOutOfRangeException("maxBlobSize", string.Format("The maximum PageBlob size must be between {0} and {1} bytes.", (long)0, 8796093022208L));
				}
				if (maxBlobSize.Value % (long)512 != (long)0)
				{
					string str = string.Format("The maximum blob size ({0}) does not fall on a page boundary.", maxBlobSize.Value);
					throw new XStoreArgumentException(str);
				}
			}
		}

		public static void ValidateMaxContainers(int maxContainers)
		{
			if (maxContainers < 0 || maxContainers > 5000)
			{
				throw new XStoreArgumentOutOfRangeException("maxContainers", string.Format("MaxContainers parameter should be >= 0 and <= {0}", 5000));
			}
		}

		private static void ValidateMD5(byte[] md5)
		{
			if (md5 != null && (long)((int)md5.Length) != (long)16)
			{
				throw new MD5InvalidException(null, null);
			}
		}

		public static void ValidatePutBlobArguments(IBlobObject blob, long contentLength, long? maxBlobSize, byte[] applicationMetadata, byte[] contentMD5, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition)
		{
			StorageStampHelpers.ValidatePutBlobArguments(blob, contentLength, maxBlobSize, applicationMetadata, contentMD5, sequenceNumberUpdate, overwriteOption, condition, false, false);
		}

		public static void ValidatePutBlobArguments(IBlobObject blob, long contentLength, long? maxBlobSize, byte[] applicationMetadata, byte[] contentMD5, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed)
		{
			if (blob.Snapshot != StorageStampHelpers.RootBlobSnapshotVersion)
			{
				throw new XStoreArgumentException("This operation is only supported on the root blob.");
			}
			long num = 1099511627776L;
			if (contentLength < (long)0)
			{
				throw new ArgumentOutOfRangeException("contentLength", "contentLength must be >= 0");
			}
			if (blob.Type == BlobType.ListBlob && !isLargeBlockBlobRequest && contentLength > (long)67108864)
			{
				throw new BlobContentTooLargeException(new long?((long)67108864), null, null);
			}
			if (blob.Type == BlobType.IndexBlob && maxBlobSize.Value > num)
			{
				throw new BlobContentTooLargeException(new long?(num), null, null);
			}
			StorageStampHelpers.ValidateMaxBlobSizeForPutBlob(blob, maxBlobSize);
			StorageStampHelpers.ValidateApplicationMetadata(applicationMetadata);
			if (contentMD5 != null && (long)((int)contentMD5.Length) != (long)16)
			{
				throw new MD5InvalidException();
			}
			if (sequenceNumberUpdate != null)
			{
				if (blob.Type != BlobType.IndexBlob)
				{
					throw new XStoreArgumentException("sequenceNumberUpdate only allowed for PageBlob");
				}
				if (sequenceNumberUpdate.UpdateType != SequenceNumberUpdateType.Update)
				{
					throw new XStoreArgumentException("sequenceNumberUpdate can only have type Update for PutBlob.");
				}
				if (sequenceNumberUpdate.SequenceNumber < (long)0 || sequenceNumberUpdate.SequenceNumber > 9223372036854775807L)
				{
					throw new XStoreArgumentException("sequenceNumberUpdate sequence number must be >= 0 and < Int64.MaxValue.");
				}
			}
		}

		public static void ValidatePutBlockArguments(IBlobObject blob, byte[] blockIdentifier, long contentLength, byte[] contentMD5, IBlobObjectCondition condition)
		{
			StorageStampHelpers.ValidatePutBlockArguments(blob, blockIdentifier, contentLength, contentMD5, condition, false);
		}

		public static void ValidatePutBlockArguments(IBlobObject blob, byte[] blockIdentifier, long contentLength, byte[] contentMD5, IBlobObjectCondition condition, bool isLargeBlockBlobRequest)
		{
			if (blob.Snapshot != StorageStampHelpers.RootBlobSnapshotVersion)
			{
				throw new XStoreArgumentException("This operation is only supported on the root blob.");
			}
			if ((int)blockIdentifier.Length <= 0 || (int)blockIdentifier.Length > 64)
			{
				throw new XStoreArgumentOutOfRangeException("blockIdentifier");
			}
			if (contentLength <= (long)0)
			{
				throw new ArgumentOutOfRangeException("contentLength", "contentLength must be > 0");
			}
			if (!isLargeBlockBlobRequest && contentLength > (long)4194304)
			{
				throw new BlobContentTooLargeException(new long?((long)4194304), null, null);
			}
			StorageStampHelpers.ValidateMD5(contentMD5);
			if (condition != null)
			{
				if (condition.IfModifiedSinceTime.HasValue)
				{
					throw new XStoreArgumentException("PutBlock does not support IfModifiedSince as a condition. Only LeaseId is supported as part of the condition for PutBlock.");
				}
				if (condition.IfNotModifiedSinceTime.HasValue)
				{
					throw new XStoreArgumentException("PutBlock does not support IfNotModifiedSince as a condition. Only LeaseId is supported as part of the condition for PutBlock.");
				}
			}
		}

		public static void ValidatePutBlockListArguments(IBlobObject blob, long contentLength, byte[] applicationMetadata, byte[][] blockIdList, BlockSource[] blockSourceList, byte[] md5, IBlobObjectCondition condition, BlobServiceVersion blobServiceVersion)
		{
			if (blob.Snapshot != StorageStampHelpers.RootBlobSnapshotVersion)
			{
				throw new XStoreArgumentException("This operation is only supported on the root blob.");
			}
			if (contentLength < (long)-1)
			{
				throw new ArgumentOutOfRangeException("contentLength", "contentLength must be >= -1");
			}
			if (contentLength > 5242880000000L)
			{
				throw new BlobContentTooLargeException(new long?(5242880000000L), null, null);
			}
			StorageStampHelpers.ValidateApplicationMetadata(applicationMetadata);
			if (blockSourceList != null && blobServiceVersion < BlobServiceVersion.July09)
			{
				throw new XStoreArgumentException("blockSourceList is only allowed for verisons STG18 and up.");
			}
			int num = (blockIdList != null ? (int)blockIdList.Length : 0);
			int num1 = (blockSourceList != null ? (int)blockSourceList.Length : 0);
			if (blockIdList != null && blockSourceList != null && num != num1)
			{
				string str = string.Format("blockIdList (length {0}) and blockSourceList (length {1}) must be of the same length.", num, num1);
				throw new XStoreArgumentException(str);
			}
			if ((long)num > (long)50000)
			{
				throw new BlockListTooLongException();
			}
			StorageStampHelpers.ValidateMD5(md5);
		}

		public static void ValidateServiceMetadata(byte[] serviceMetadata)
		{
			StorageStampHelpers.ValidateServiceProperties(serviceMetadata, null, true);
		}

		public static void ValidateServiceProperties(NameValueCollection serviceMetadataCollection, string contentType)
		{
			StorageStampHelpers.ValidateServiceProperties(MetadataEncoding.Encode(serviceMetadataCollection), contentType);
		}

		public static void ValidateServiceProperties(byte[] serviceMetadata, string contentType)
		{
			StorageStampHelpers.ValidateServiceProperties(serviceMetadata, contentType, false);
		}

		private static void ValidateServiceProperties(byte[] serviceMetadata, string contentType, bool forceLengthCheck)
		{
			int num = (serviceMetadata != null ? (int)serviceMetadata.Length : 0);
			if (num + (contentType != null ? contentType.Length : 0) > StorageStampHelpers.MaxServicePropertiesLength)
			{
				int maxServicePropertiesLength = StorageStampHelpers.MaxServicePropertiesLength;
				throw new ObjectMetadataOverLimitException(string.Concat("The properties should be at most ", maxServicePropertiesLength.ToString(), " characters."));
			}
		}

		public static void ValidateStorageDomainLabel(string storageDomainLabel)
		{
			if (storageDomainLabel.Length < 1 || storageDomainLabel.Length > 63)
			{
				throw new ArgumentOutOfRangeException("storageDomainName", string.Format("The length {0} of the label {1} in storage domain name is invalid.", storageDomainLabel.Length, storageDomainLabel));
			}
			int invalidCharInName = StorageStampHelpers.GetInvalidCharInName(storageDomainLabel, StorageStampHelpers.DOMAIN_LABEL_ALLOWED_PATTERN, true);
			if (invalidCharInName >= 0)
			{
				throw new ArgumentOutOfRangeException("storageDomainName", string.Format("The character '{0}' at index {1} is not allowed in the storage domain label {2}", StorageStampHelpers.MakeXmlFriendlyString(storageDomainLabel[invalidCharInName]), invalidCharInName, storageDomainLabel));
			}
		}
	}
}