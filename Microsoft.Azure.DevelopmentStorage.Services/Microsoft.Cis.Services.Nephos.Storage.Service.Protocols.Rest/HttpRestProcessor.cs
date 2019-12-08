using AsyncHelper;
using AsyncHelper.Streams;
using MeasurementEvents;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.Protocols.Rest
{
	public class HttpRestProcessor : BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>
	{
		private const long SizeOfEachBlockIdInXml = 256L;

		private const long MaxContentLengthForXmlBlockList = 12800000L;

		private const long SizeOfEachSasIdentifier = 2000L;

		private const long MaxContentLengthForXmlSetContainerAcl = 10000L;

		private const int ApproximateBytesForBlockId = 128;

		private const int ApproximateBytesForBlockSize = 5;

		private const long PageSize = 512L;

		protected const int ApproximateBytesForStartOrEndNumber = 5;

		protected const int MaxLengthForRequestCopySource = 2048;

		private const int BufferSizeForBufferedMemoryStream = 8192;

		private const int BufferSizeForBufferedMemoryStream17KB = 17408;

		private const long Gigabyte = 1073741824L;

		protected const long Terabyte = 1099511627776L;

		private Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager;

		private static int ApproximateByteSizePerBlock;

		private static int ApproximateByteSizePerPageRange;

		private static int SingleGetPageRangeListMaxPageRanges;

		private static TimeSpan DefaultLeaseDuration;

		private static TimeSpan LeaseDurationMinSeconds;

		private static TimeSpan LeaseDurationMaxSeconds;

		protected readonly static TimeSpan DefaultMaxAllowedTimeoutForCopyBlob;

		private static BlockListTypes DefaultBlockListType;

		private static List<string> ValidListBlobsIncludeQueryParamValues;

		protected static List<string> ValidListContainersIncludeQueryParamValues;

		protected static List<string> ValidListFileContainersIncludeQueryParamValuesOctober16;

		private static List<ContinuationTokenVersion> ValidListBlobsContinuationTokenVersions;

		protected static List<Version> DefaultNonSupportedHttpVersions;

		private readonly TimeSpan PutBlockListTimeout = TimeSpan.FromSeconds(60);

		private readonly TimeSpan GetBlockListInitialTimeout = TimeSpan.FromSeconds(60);

		private readonly TimeSpan GetPageRangeListInitialTimeout = TimeSpan.FromSeconds(60);

		protected override bool AdjustRequestVersionBasedOnContainerAclSettings
		{
			get
			{
				NephosAssertionException.Assert(base.UriComponents != null, "UriComponents should have been set");
				bool flag = (string.IsNullOrEmpty(base.UriComponents.AccountName) ? false : !string.IsNullOrEmpty(base.UriComponents.ContainerName));
				bool flag1 = (base.Method == RestMethod.GET || base.Method == RestMethod.HEAD ? base.IsRequestAnonymousAndUnversioned : false);
				if (flag)
				{
					return flag1;
				}
				return false;
			}
		}

		public static HttpProcessorConfiguration HttpProcessorConfigurationDefaultInstance
		{
			get;
			set;
		}

		protected override string ServerResponseHeaderValue
		{
			get
			{
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					return "Windows-Azure-Blob/1.0";
				}
				return "Blob Service Version 1.0";
			}
		}

		static HttpRestProcessor()
		{
			HttpRestProcessor.ApproximateByteSizePerBlock = "Block".Length * 2 + "Name".Length * 2 + "Size".Length * 2 + 128 + 5;
			HttpRestProcessor.ApproximateByteSizePerPageRange = "PageRange".Length * 2 + "Start".Length * 2 + "End".Length * 2 + 10;
			HttpRestProcessor.SingleGetPageRangeListMaxPageRanges = 10000;
			HttpRestProcessor.DefaultLeaseDuration = TimeSpan.FromMinutes(1);
			HttpRestProcessor.LeaseDurationMinSeconds = TimeSpan.FromSeconds(15);
			HttpRestProcessor.LeaseDurationMaxSeconds = TimeSpan.FromSeconds(60);
			HttpRestProcessor.DefaultMaxAllowedTimeoutForCopyBlob = TimeSpan.FromSeconds(90);
			HttpRestProcessor.DefaultBlockListType = BlockListTypes.Committed;
			string[] strArrays = new string[] { "metadata", "snapshots", "uncommittedblobs", "copy", "deleted" };
			HttpRestProcessor.ValidListBlobsIncludeQueryParamValues = new List<string>(strArrays);
			HttpRestProcessor.ValidListContainersIncludeQueryParamValues = new List<string>(new string[] { "metadata" });
			string[] strArrays1 = new string[] { "metadata", "snapshots" };
			HttpRestProcessor.ValidListFileContainersIncludeQueryParamValuesOctober16 = new List<string>(strArrays1);
			ContinuationTokenVersion[] continuationTokenVersionArray = new ContinuationTokenVersion[] { ContinuationTokenVersion.VersionOne, ContinuationTokenVersion.VersionTwo };
			HttpRestProcessor.ValidListBlobsContinuationTokenVersions = new List<ContinuationTokenVersion>(continuationTokenVersionArray);
			Version[] version10 = new Version[] { HttpVersion.Version10 };
			HttpRestProcessor.DefaultNonSupportedHttpVersions = new List<Version>(version10);
		}

		protected HttpRestProcessor(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transfromProviderException, IIpThrottlingTable ipThrottlingTable) : base(requestContext, storageManager, authenticationManager, configuration, transfromProviderException, ipThrottlingTable)
		{
			NephosAssertionException.Assert(serviceManager != null);
			this.serviceManager = serviceManager;
		}

		private IEnumerator<IAsyncResult> AbortCopyBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetCopyId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected copy ID to have a value or for a protocol exception to be thrown.");
			if (this.GetCopyAction(true) != CopyAction.Abort)
			{
				throw new InvalidHeaderProtocolException("x-ms-copy-action", base.RequestHeadersCollection["x-ms-copy-action"]);
			}
			BlobObjectCondition blobObjectCondition = new BlobObjectCondition()
			{
				LeaseId = this.GetLeaseId(false)
			};
			IAsyncResult asyncResult = this.serviceManager.BeginAbortCopyBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nullable.Value, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.AbortCopyBlobImpl"));
			yield return asyncResult;
			this.serviceManager.EndAbortCopyBlob(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.NoContent);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> AcquireBlobLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			TimeSpan timeSpan = this.GetLeaseDuration();
			Guid? nullable = this.GetProposedLeaseId(false);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Guid? nullable1 = null;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			DateTime? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable1, nullable2, nullable3, BlobType.None, false, false, true, false, nullable4, null, nullable5);
			IAsyncResult asyncResult = this.serviceManager.BeginAcquireBlobLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, LeaseType.ReadWrite, timeSpan, nullable, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.AcquireBlobLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndAcquireBlobLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> AcquireContainerLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			TimeSpan timeSpan = this.GetLeaseDuration();
			Guid? nullable = this.GetProposedLeaseId(false);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ContainerCondition containerCondition = BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ConvertToContainerCondition(this.operationContext.RequestConditionInformation);
			IAsyncResult asyncResult = this.serviceManager.BeginAcquireContainerLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, LeaseType.ReadWrite, timeSpan, nullable, containerCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.AcquireContainerLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndAcquireContainerLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		protected void AddBlobAppendOffsetToResponse(IAppendBlockResult result)
		{
			base.Response.AddHeader("x-ms-blob-append-offset", result.AppendOffset.ToString());
		}

		protected void AddCommittedBlockCountToResponse(IAppendBlockResult result)
		{
			base.Response.AddHeader("x-ms-blob-committed-block-count", result.CommittedBlockCount.ToString());
		}

		protected void AddContentCrc64Md5ToResponse(ICrc64Md5Result crc64Md5Result)
		{
			if (!base.SupportCrc64)
			{
				NephosAssertionException.Assert(crc64Md5Result.ContentMD5 != null);
				this.AddContentMD5ToResponse(crc64Md5Result.ContentMD5, "Content-MD5");
			}
			else
			{
				if (base.RequestHeadersCollection["x-ms-content-crc64"] != null)
				{
					NephosAssertionException.Assert(crc64Md5Result.ContentCrc64.HasValue);
					this.AddNephosContentCrc64ToResponse(crc64Md5Result.ContentCrc64);
					return;
				}
				if (base.RequestHeadersCollection["Content-MD5"] != null)
				{
					NephosAssertionException.Assert(crc64Md5Result.ContentMD5 != null);
					this.AddContentMD5ToResponse(crc64Md5Result.ContentMD5, "Content-MD5");
					return;
				}
			}
		}

		protected void AddContentMD5ToResponse(byte[] contentMD5, string headerName)
		{
			string base64String = Convert.ToBase64String(contentMD5);
			base.Response.AddHeader(headerName, base64String);
		}

		protected void AddCopyInfoToResponse(CopyBlobOperationInfo copyInfo)
		{
			Guid copyId = copyInfo.CopyId;
			NephosAssertionException.Assert(true, "CopyId not expected to be null");
			base.Response.AddHeader("x-ms-copy-id", copyInfo.CopyId.ToString());
			base.Response.AddHeader("x-ms-copy-status", copyInfo.CopyStatus);
		}

		protected void AddLastModifiedAndETagToResponse(DateTime lastModified)
		{
			string httpString = HttpUtilities.ConvertDateTimeToHttpString(lastModified);
			string eTag = BasicHttpProcessor.GetETag(lastModified, base.RequestContext.IsRequestVersionAtLeastAugust11);
			base.Response.AddHeader("Last-Modified", httpString);
			base.Response.AddHeader("ETag", eTag);
		}

		private void AddLeaseStatusToResponse(ILeaseInfo leaseInfo)
		{
			NephosAssertionException.Assert(leaseInfo != null, "Blob's lease info not populated!.");
			if (leaseInfo.Type == LeaseType.ReadWrite && leaseInfo.Duration.HasValue)
			{
				TimeSpan? duration = leaseInfo.Duration;
				TimeSpan zero = TimeSpan.Zero;
				if ((duration.HasValue ? duration.GetValueOrDefault() <= zero : true))
				{
					goto Label1;
				}
				base.Response.AddHeader("x-ms-lease-status", "locked");
				goto Label0;
			}
		Label1:
			base.Response.AddHeader("x-ms-lease-status", "unlocked");
		Label0:
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12 && leaseInfo.State.HasValue)
			{
				base.Response.AddHeader("x-ms-lease-state", LeaseStateStrings.LeaseStates[(int)leaseInfo.State.Value]);
				if (leaseInfo.State.Equals(LeaseState.Leased))
				{
					TimeSpan? nullable = leaseInfo.Duration;
					TimeSpan timeSpan = TimeSpan.FromSeconds(4294967295);
					if ((!nullable.HasValue ? false : nullable.GetValueOrDefault() == timeSpan))
					{
						base.Response.AddHeader("x-ms-lease-duration", "infinite");
						return;
					}
					base.Response.AddHeader("x-ms-lease-duration", "fixed");
				}
			}
		}

		protected void AddNephosContentCrc64ToResponse(long? contentCrc64)
		{
			string base64String;
			if (contentCrc64.HasValue)
			{
				base64String = Convert.ToBase64String(BitConverter.GetBytes(contentCrc64.Value));
			}
			else
			{
				base64String = null;
			}
			base.Response.AddHeader("x-ms-content-crc64", base64String);
		}

		protected void AddQueryResponseHeadersForSas()
		{
			string empty = string.Empty;
			string item = string.Empty;
			try
			{
				empty = "rscc";
				item = base.RequestQueryParameters[empty];
				if (item != null)
				{
					base.Response.Headers["Cache-Control"] = item;
				}
				empty = "rscd";
				item = base.RequestQueryParameters[empty];
				if (item != null)
				{
					base.Response.Headers["Content-Disposition"] = item;
				}
				empty = "rsce";
				item = base.RequestQueryParameters[empty];
				if (item != null)
				{
					base.Response.Headers["Content-Encoding"] = item;
				}
				empty = "rscl";
				item = base.RequestQueryParameters[empty];
				if (item != null)
				{
					base.Response.Headers["Content-Language"] = item;
				}
				empty = "rsct";
				item = base.RequestQueryParameters[empty];
				if (item != null)
				{
					base.Response.Headers["Content-Type"] = item;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] message = new object[] { empty, item, exception.Message };
				error.Log("SAS query parameter response headers has invalid characters.\r\n                                            Query param name='{0}', query param value='{1}', exception='{2}'", message);
				throw new InvalidQueryParameterProtocolException(empty, item, "HTTP query parameter values contain invalid characters");
			}
		}

		protected void AddRequestServerEncryptionStatusToResponse(bool isWriteEncrypted)
		{
			if (base.RequestContext.IsRequestVersionAtLeastDecember15)
			{
				base.Response.AddHeader("x-ms-request-server-encrypted", (isWriteEncrypted ? "true" : "false"));
			}
		}

		private void AddSequenceNumberToResponse(long sequenceNumber)
		{
			object[] objArray = new object[] { sequenceNumber };
			NephosAssertionException.Assert(sequenceNumber >= (long)0, "Blob sequence number ({0}) must be >= 0!", objArray);
			base.Response.AddHeader("x-ms-blob-sequence-number", sequenceNumber.ToString());
		}

		protected void AddSnapshotTimestampToResponse(DateTime snapshotTimestamp)
		{
			string httpString = HttpUtilities.ConvertSnapshotDateTimeToHttpString(snapshotTimestamp);
			base.Response.AddHeader("x-ms-snapshot", httpString);
		}

		protected TimeSpan AdjustTimeoutForListResult(INephosBaseOperationMeasurementEvent operationEvent, long approximateByteCountPerItem, long itemCount, Duration duration, TimeSpan currentTotalTimeout)
		{
			long num = itemCount * approximateByteCountPerItem;
			TimeSpan sizeBasedTimeout = BlobObjectHelper.GetSizeBasedTimeout(num, BlobObjectHelper.GetDataRateForTimeout);
			TimeSpan timeSpan = duration.Remaining(currentTotalTimeout) + sizeBasedTimeout;
			this.operationContext.MaxAllowedTimeout = this.SafeTimeSpanAdd(this.operationContext.MaxAllowedTimeout, sizeBasedTimeout);
			this.operationContext.IsSizeBased = true;
			TimeSpan timeSpan1 = this.operationContext.OperationDuration.Remaining(this.operationContext.UserTimeout);
			if (timeSpan1 > timeSpan)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] totalSeconds = new object[] { (long)timeSpan1.TotalSeconds, (long)timeSpan.TotalSeconds, num, BlobObjectHelper.GetDataRateForTimeout };
				verbose.Log("SecurityWarning: Ignoring new timeout since it reduces timeout from {0}s to {1}s on additional {2}bytes and rate {3} bytes/min.", totalSeconds);
			}
			else
			{
				timeSpan = timeSpan1;
			}
			IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] maxAllowedTimeout = new object[] { timeSpan, timeSpan1, this.operationContext.MaxAllowedTimeout, sizeBasedTimeout, BlobObjectHelper.GetDataRateForTimeout, num, itemCount, approximateByteCountPerItem, duration.Remaining(currentTotalTimeout) };
			stringDataEventStream.Log("Setting timeout to '{0}' RemainingUserTimeout='{1}', MaxTimeout='{2}', Size-Based Timeout='{3}', DataRate={4} bytes/min, Total byte count={5} Items={6} Bytes Per Item {7}. Previous RemainingTimeout='{8}'.)", maxAllowedTimeout);
			return timeSpan;
		}

		private IEnumerator<IAsyncResult> AppendBlockImpl(AsyncIteratorContext<NoResults> async)
		{
			long? requestCrc64;
			string str;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			AppendBlockMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as AppendBlockMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			if (base.RequestContentLength > (long)4194304)
			{
				throw new BlobContentTooLargeException(new long?((long)4194304), "The block exceeds the maximum block size.", null);
			}
			this.EnsureCrc64AndMd5HeaderAreMutuallyExclusive(false);
			long? maxSizeBlobConditionFromRequest = this.GetMaxSizeBlobConditionFromRequest();
			long? blobAppendPositionConditionFromRequest = this.GetBlobAppendPositionConditionFromRequest();
			this.ValidateAppendBlockConditionArguments(maxSizeBlobConditionFromRequest, blobAppendPositionConditionFromRequest);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			operationMeasurementEvent.BlockSize = base.RequestContentLength;
			this.operationContext.MaxAllowedTimeout = BlobObjectHelper.GetSizeBasedTimeout(base.RequestContentLength, TimeSpan.MaxValue, this.MinimumSizeBasedTimeout, ServiceConstants.DefaultPutDataRateForTimeout);
			base.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
			ComparisonOperator? nullable1 = null;
			long? nullable2 = null;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable, nullable1, nullable2, BlobType.None, false, false, false, false, nullable3, null, nullable4);
			IAppendBlockResult appendBlockResult = null;
			using (Stream stream = base.GenerateMeasuredRequestStream())
			{
				Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
				IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
				string accountName = this.operationContext.AccountName;
				string containerName = this.operationContext.ContainerName;
				string blobOrFilePathName = this.operationContext.BlobOrFilePathName;
				Stream stream1 = stream;
				long num = base.RequestContentLength;
				if (base.SupportCrc64)
				{
					requestCrc64 = base.GetRequestCrc64("x-ms-content-crc64");
				}
				else
				{
					requestCrc64 = null;
				}
				IAsyncResult asyncResult = serviceManager.BeginAppendBlock(callerIdentity, accountName, containerName, blobOrFilePathName, stream1, num, requestCrc64, base.GetRequestMD5("Content-MD5"), base.SupportCrc64, blobObjectCondition, maxSizeBlobConditionFromRequest, blobAppendPositionConditionFromRequest, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.AppendBlockImpl"));
				yield return asyncResult;
				appendBlockResult = this.serviceManager.EndAppendBlock(asyncResult);
			}
			this.AddContentCrc64Md5ToResponse(appendBlockResult);
			this.AddLastModifiedAndETagToResponse(appendBlockResult.LastModifiedTime);
			this.AddBlobAppendOffsetToResponse(appendBlockResult);
			this.AddCommittedBlockCountToResponse(appendBlockResult);
			this.AddRequestServerEncryptionStatusToResponse(appendBlockResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		protected override void ApplyResourceToMeasurementEvent(INephosBaseMeasurementEvent measurementEvent)
		{
			IBlobMeasurementEvent accountName = measurementEvent as IBlobMeasurementEvent;
			if (accountName != null)
			{
				accountName.AccountName = this.operationContext.AccountName;
				accountName.ContainerName = this.operationContext.ContainerName;
				accountName.BlobName = this.operationContext.BlobOrFilePathName;
			}
			base.ApplyResourceToMeasurementEvent(measurementEvent);
		}

		private IEnumerator<IAsyncResult> BlobPreflightRequestHandlerImpl(AsyncIteratorContext<NoResults> async)
		{
			base.HandlePreflightCorsRequest(this.storageAccount.ServiceMetadata.BlobAnalyticsSettings);
			yield break;
		}

		private IEnumerator<IAsyncResult> BreakBlobLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			TimeSpan? leaseBreakPeriod = this.GetLeaseBreakPeriod();
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Guid? nullable = null;
			ComparisonOperator? nullable1 = null;
			long? nullable2 = null;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable, nullable1, nullable2, BlobType.None, false, false, true, false, nullable3, null, nullable4);
			IAsyncResult asyncResult = this.serviceManager.BeginBreakBlobLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, leaseBreakPeriod, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.BreakBlobLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndBreakBlobLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Duration);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> BreakContainerLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			TimeSpan? leaseBreakPeriod = this.GetLeaseBreakPeriod();
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ContainerCondition containerCondition = BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ConvertToContainerCondition(this.operationContext.RequestConditionInformation);
			IAsyncResult asyncResult = this.serviceManager.BeginBreakContainerLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, leaseBreakPeriod, containerCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.BreakContainerLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndBreakContainerLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Duration);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ChangeBlobLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			Guid? nullable1 = this.GetProposedLeaseId(true);
			NephosAssertionException.Assert(nullable1.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Guid? nullable2 = null;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			DateTime? nullable5 = null;
			Guid? nullable6 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable2, nullable3, nullable4, BlobType.None, false, false, true, false, nullable5, null, nullable6);
			IAsyncResult asyncResult = this.serviceManager.BeginChangeBlobLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nullable.Value, nullable1.Value, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.ChangeBlobLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndChangeBlobLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ChangeContainerLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			Guid? nullable1 = this.GetProposedLeaseId(true);
			NephosAssertionException.Assert(nullable1.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ContainerCondition containerCondition = BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ConvertToContainerCondition(this.operationContext.RequestConditionInformation);
			IAsyncResult asyncResult = this.serviceManager.BeginChangeContainerLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nullable.Value, nullable1.Value, containerCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.ChangeContainerLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndChangeContainerLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		protected override BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl ChooseRestMethodHandler(RestMethod method)
		{
			if (method == RestMethod.Unknown)
			{
				throw new UnknownVerbProtocolException(base.HttpVerb);
			}
			base.ThrowIfApiNotSupportedForVersion(base.RequestSettings != null);
			string subResource = this.operationContext.SubResource;
			if (method == RestMethod.OPTIONS)
			{
				this.operationContext.OperationMeasurementEvent = new BlobPreflightRequestMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.BlobPreflightApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.BlobPreflightRequestHandlerImpl);
			}
			if (!this.operationContext.ResourceIsBlobOrFilePath)
			{
				if (!this.operationContext.ResourceIsContainer)
				{
					if (!this.operationContext.ResourceIsAccount)
					{
						throw new InvalidUrlProtocolException(base.RequestUrl.AbsolutePath);
					}
					if (subResource == null)
					{
						throw new InvalidQueryParameterProtocolException("comp", subResource, null);
					}
					if (Comparison.StringEqualsIgnoreCase(subResource, "list"))
					{
						if (method != RestMethod.GET)
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.GetOnlyOperations);
						}
						this.operationContext.OperationMeasurementEvent = new ListContainersMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ListContainersApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ListContainersImpl);
					}
					if (Comparison.StringEqualsIgnoreCase(subResource, "bloblist"))
					{
						if (method != RestMethod.GET)
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.GetOnlyOperations);
						}
						this.operationContext.OperationMeasurementEvent = new ListBlobsMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ListBlobsApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ListBlobsImpl);
					}
					if (!this.operationContext.IsResourceTypeService || !Comparison.StringEqualsIgnoreCase(subResource, "properties"))
					{
						if (!this.operationContext.IsResourceTypeService || !Comparison.StringEqualsIgnoreCase(subResource, "stats"))
						{
							throw new InvalidQueryParameterProtocolException("comp", subResource, null);
						}
						if (method != RestMethod.GET)
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.GetOnlyOperations);
						}
						this.operationContext.OperationMeasurementEvent = new GetBlobServiceStatsMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobServiceStatsApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobServiceStatsImpl);
					}
					switch (method)
					{
						case RestMethod.GET:
						{
							this.operationContext.OperationMeasurementEvent = new GetBlobServicePropertiesMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobServicePropertiesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobServicePropertiesImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetBlobServicePropertiesMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetBlobServicePropertiesApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SetBlobServicePropertiesImpl);
						}
					}
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteOperations);
				}
				if (subResource == null)
				{
					switch (method)
					{
						case RestMethod.GET:
						{
							if (!this.operationContext.IsUnversionedRequest || this.operationContext.IsResourceTypeContainer)
							{
								this.operationContext.OperationMeasurementEvent = new GetContainerPropertiesMeasurementEvent();
								base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerPropertiesApiEnabled);
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerPropertiesImpl);
							}
							this.operationContext.BlobOrFilePathName = this.operationContext.ContainerName;
							this.operationContext.ContainerName = "$root";
							this.operationContext.OperationMeasurementEvent = new GetBlobMeasurementEvent();
							this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent = new GetContainerPropertiesMeasurementEvent();
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobOrGetContainerPropertiesImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new CreateContainerMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.CreateContainerApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.CreateContainerImpl);
						}
						case RestMethod.POST:
						case RestMethod.MERGE:
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteDeleteOperations);
						}
						case RestMethod.DELETE:
						{
							this.operationContext.OperationMeasurementEvent = new DeleteContainerMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.DeleteContainerApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.DeleteContainerImpl);
						}
						case RestMethod.HEAD:
						{
							if (!this.operationContext.IsUnversionedRequest || this.operationContext.IsResourceTypeContainer)
							{
								this.operationContext.OperationMeasurementEvent = new GetContainerPropertiesMeasurementEvent();
								base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerPropertiesApiEnabled);
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerPropertiesImpl);
							}
							this.operationContext.BlobOrFilePathName = this.operationContext.ContainerName;
							this.operationContext.ContainerName = "$root";
							this.operationContext.OperationMeasurementEvent = new GetBlobPropertiesMeasurementEvent();
							this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent = new GetContainerPropertiesMeasurementEvent();
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobPropertiesOrGetContainerPropertiesImpl);
						}
						default:
						{
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteDeleteOperations);
						}
					}
				}
				if (Comparison.StringEqualsIgnoreCase(subResource, "list"))
				{
					if (method != RestMethod.GET)
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.GetOnlyOperations);
					}
					this.operationContext.OperationMeasurementEvent = new ListBlobsMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ListBlobsApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ListBlobsImpl);
				}
				if (Comparison.StringEqualsIgnoreCase(subResource, "metadata"))
				{
					RestMethod restMethod = method;
					switch (restMethod)
					{
						case RestMethod.GET:
						{
							if (!this.operationContext.IsUnversionedRequest || this.operationContext.IsResourceTypeContainer)
							{
								this.operationContext.OperationMeasurementEvent = new GetContainerMetadataMeasurementEvent();
								base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerMetadataApiEnabled);
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerMetadataImpl);
							}
							this.operationContext.BlobOrFilePathName = this.operationContext.ContainerName;
							this.operationContext.ContainerName = "$root";
							this.operationContext.OperationMeasurementEvent = new GetBlobMetadataMeasurementEvent();
							this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent = new GetContainerMetadataMeasurementEvent();
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataOrGetContainerMetadataImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetContainerMetadataMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetContainerMetadataApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SetContainerMetadataImpl);
						}
						default:
						{
							if (restMethod == RestMethod.HEAD)
							{
								if (!this.operationContext.IsUnversionedRequest || this.operationContext.IsResourceTypeContainer)
								{
									this.operationContext.OperationMeasurementEvent = new GetContainerMetadataMeasurementEvent();
									base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerMetadataApiEnabled);
									return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerMetadataImpl);
								}
								this.operationContext.BlobOrFilePathName = this.operationContext.ContainerName;
								this.operationContext.ContainerName = "$root";
								this.operationContext.OperationMeasurementEvent = new GetBlobMetadataMeasurementEvent();
								this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent = new GetContainerMetadataMeasurementEvent();
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataOrGetContainerMetadataImpl);
							}
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteOperations);
						}
					}
				}
				if (Comparison.StringEqualsIgnoreCase(subResource, "acl"))
				{
					RestMethod restMethod1 = method;
					switch (restMethod1)
					{
						case RestMethod.GET:
						{
							this.operationContext.OperationMeasurementEvent = new GetContainerAclMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerAclApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerAclImpl);
						}
						case RestMethod.PUT:
						{
							this.operationContext.OperationMeasurementEvent = new SetContainerAclMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetContainerAclApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SetContainerAclImpl);
						}
						default:
						{
							if (restMethod1 == RestMethod.HEAD)
							{
								this.operationContext.OperationMeasurementEvent = new GetContainerAclMeasurementEvent();
								base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetContainerAclApiEnabled);
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerAclImpl);
							}
							throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteOperations);
						}
					}
				}
				if (!Comparison.StringEqualsIgnoreCase(subResource, "lease"))
				{
					throw new InvalidQueryParameterProtocolException("comp", subResource, null);
				}
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				string item = base.RequestHeadersCollection["x-ms-lease-action"];
				if (string.IsNullOrEmpty(item))
				{
					throw new RequiredHeaderNotPresentProtocolException(item);
				}
				if (item.Equals("acquire", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsAcquireLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new AcquireContainerLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.AcquireContainerLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.AcquireContainerLeaseImpl);
				}
				if (item.Equals("break", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsBreakLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new BreakContainerLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.BreakContainerLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.BreakContainerLeaseImpl);
				}
				if (item.Equals("change", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsChangeLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new ChangeContainerLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ChangeContainerLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ChangeContainerLeaseImpl);
				}
				if (item.Equals("renew", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsRenewLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new RenewContainerLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.RenewContainerLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.RenewContainerLeaseImpl);
				}
				if (!item.Equals("release", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidHeaderProtocolException("x-ms-lease-action", item);
				}
				this.operationContext.IsReleaseLeaseOperation = true;
				this.operationContext.OperationMeasurementEvent = new ReleaseContainerLeaseMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ReleaseContainerLeaseApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ReleaseContainerLeaseImpl);
			}
			if (subResource == null)
			{
				switch (method)
				{
					case RestMethod.GET:
					{
						this.operationContext.OperationMeasurementEvent = new GetBlobMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobImpl);
					}
					case RestMethod.PUT:
					{
						if (!string.IsNullOrEmpty(base.RequestCopySource))
						{
							this.operationContext.OperationMeasurementEvent = new CopyBlobMeasurementEvent();
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.CopyBlobApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.CopyBlobImpl);
						}
						this.operationContext.OperationMeasurementEvent = new PutBlobMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.PutBlobApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.PutBlobImpl);
					}
					case RestMethod.POST:
					case RestMethod.MERGE:
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteDeleteOperations);
					}
					case RestMethod.DELETE:
					{
						this.operationContext.OperationMeasurementEvent = new DeleteBlobMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.DeleteBlobApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.DeleteBlobImpl);
					}
					case RestMethod.HEAD:
					{
						this.operationContext.OperationMeasurementEvent = new GetBlobPropertiesMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobPropertiesApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobPropertiesImpl);
					}
					default:
					{
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteDeleteOperations);
					}
				}
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "properties"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				this.operationContext.OperationMeasurementEvent = new SetBlobPropertiesMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetBlobPropertiesApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SetBlobPropertiesImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "metadata"))
			{
				RestMethod restMethod2 = method;
				switch (restMethod2)
				{
					case RestMethod.GET:
					{
						this.operationContext.OperationMeasurementEvent = new GetBlobMetadataMeasurementEvent();
						if (base.IsRequestAnonymous && base.RequestRestVersion.Equals("2009-07-17", StringComparison.OrdinalIgnoreCase))
						{
							this.operationContext.TemporaryGetContainerMetadataMeasurementEvent = new GetContainerMetadataMeasurementEvent();
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataOrGetContainerMetadataImpl);
						}
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobMetadataApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataImpl);
					}
					case RestMethod.PUT:
					{
						this.operationContext.OperationMeasurementEvent = new SetBlobMetadataMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SetBlobMetadataApiEnabled);
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SetBlobMetadataImpl);
					}
					default:
					{
						if (restMethod2 == RestMethod.HEAD)
						{
							this.operationContext.OperationMeasurementEvent = new GetBlobMetadataMeasurementEvent();
							if (base.IsRequestAnonymous && base.RequestRestVersion.Equals("2009-07-17", StringComparison.OrdinalIgnoreCase))
							{
								this.operationContext.TemporaryGetContainerMetadataMeasurementEvent = new GetContainerMetadataMeasurementEvent();
								return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataOrGetContainerMetadataImpl);
							}
							base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlobMetadataApiEnabled);
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataImpl);
						}
						throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteOperations);
					}
				}
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "block"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				this.operationContext.OperationMeasurementEvent = new PutBlockMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.PutBlockApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.PutBlockImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "appendblock"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				this.operationContext.OperationMeasurementEvent = new AppendBlockMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.AppendBlockApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.AppendBlockImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "page"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				string str = base.RequestHeadersCollection["x-ms-page-write"];
				if (string.IsNullOrEmpty(str))
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-page-write");
				}
				if (str.Equals("clear", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.OperationMeasurementEvent = new ClearPageMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ClearPageApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ClearPageImpl);
				}
				if (!str.Equals("update", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidHeaderProtocolException("x-ms-page-write", str);
				}
				this.operationContext.OperationMeasurementEvent = new PutPageMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.PutPageApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.PutPageImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "lease"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteOperations);
				}
				string item1 = base.RequestHeadersCollection["x-ms-lease-action"];
				if (string.IsNullOrEmpty(item1))
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-lease-action");
				}
				if (item1.Equals("acquire", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsAcquireLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new AcquireBlobLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.AcquireBlobLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.AcquireBlobLeaseImpl);
				}
				if (item1.Equals("renew", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsRenewLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new RenewBlobLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.RenewBlobLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.RenewBlobLeaseImpl);
				}
				if (item1.Equals("release", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsReleaseLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new ReleaseBlobLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ReleaseBlobLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ReleaseBlobLeaseImpl);
				}
				if (item1.Equals("break", StringComparison.OrdinalIgnoreCase))
				{
					this.operationContext.IsBreakLeaseOperation = true;
					this.operationContext.OperationMeasurementEvent = new BreakBlobLeaseMeasurementEvent();
					base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.BreakBlobLeaseApiEnabled);
					return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.BreakBlobLeaseImpl);
				}
				if (!item1.Equals("change", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidHeaderProtocolException("x-ms-lease-action", item1);
				}
				this.operationContext.IsChangeLeaseOperation = true;
				this.operationContext.OperationMeasurementEvent = new ChangeBlobLeaseMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.ChangeBlobLeaseApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.ChangeBlobLeaseImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "pagelist"))
			{
				if (method != RestMethod.GET)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.GetOnlyOperations);
				}
				this.operationContext.OperationMeasurementEvent = new GetPageRegionsMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetPageRangesApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetPageRangeListImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "blocklist"))
			{
				switch (method)
				{
					case RestMethod.GET:
					{
						this.operationContext.OperationMeasurementEvent = new GetBlockListMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.GetBlockListApiEnabled);
						if (base.RequestSettings.UseNewGetBlockListImplementation)
						{
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlockListImplV2);
						}
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlockListImpl);
					}
					case RestMethod.PUT:
					{
						this.operationContext.OperationMeasurementEvent = new PutBlockListMeasurementEvent();
						base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.PutBlockListApiEnabled);
						if (base.RequestSettings.UseNewPutBlockListImplementation)
						{
							return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.CommitBlobImplV2);
						}
						return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.CommitBlobImpl);
					}
				}
				throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ReadWriteNoHeadOperations);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "snapshot"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				this.operationContext.OperationMeasurementEvent = new SnapshotBlobMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.SnapshotBlobApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.SnapshotBlobImpl);
			}
			if (Comparison.StringEqualsIgnoreCase(subResource, "copy"))
			{
				if (method != RestMethod.PUT)
				{
					throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
				}
				this.operationContext.OperationMeasurementEvent = new AbortCopyBlobMeasurementEvent();
				base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.AbortCopyBlobApiEnabled);
				return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.AbortCopyBlobImpl);
			}
			if (!Comparison.StringEqualsIgnoreCase(subResource, "incrementalcopy"))
			{
				throw new InvalidQueryParameterProtocolException("comp", subResource, null);
			}
			if (method != RestMethod.PUT)
			{
				throw new VerbNotSupportedProtocolException(method, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.WriteOperations);
			}
			this.operationContext.OperationMeasurementEvent = new IncrementalCopyBlobMeasurementEvent();
			base.ThrowIfApiNotSupportedForVersion(base.RequestSettings.IncrementalCopyBlobApiEnabled);
			return new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.IncrementalCopyBlobImpl);
		}

		private IEnumerator<IAsyncResult> ClearPageImpl(AsyncIteratorContext<NoResults> async)
		{
			string str;
			long num;
			long num1;
			ComparisonOperator? nullable;
			long? nullable1;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable2 = this.GetLeaseId(false);
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as ClearPageMeasurementEvent != null);
			string str1 = HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str);
			if (string.IsNullOrEmpty(str1))
			{
				throw new RequiredHeaderNotPresentProtocolException("Range");
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength != (long)0)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] requestContentLength = new object[] { base.RequestContentLength };
				verbose.Log("Failing the ClearPage since ContentLength is not equal to 0 (value is {0}).", requestContentLength);
				throw new InvalidHeaderProtocolException("Content-Length", base.RequestHeadersCollection["Content-Length"]);
			}
			if (base.SupportCrc64 && !string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-content-crc64"]))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("x-ms-content-crc64 is not allowed for ClearPage.");
				throw new InvalidHeaderProtocolException("x-ms-content-crc64", base.RequestHeadersCollection["x-ms-content-crc64"]);
			}
			if (!string.IsNullOrEmpty(base.RequestHeadersCollection["Content-MD5"]))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Content-MD5 is not allowed for ClearPage.");
				throw new InvalidHeaderProtocolException("Content-MD5", base.RequestHeadersCollection["Content-MD5"]);
			}
			this.Range = str1;
			if (!HttpRestProcessor.GetRangeFromRequest(str1, out num, out num1))
			{
				throw new InvalidHeaderProtocolException("Range", base.RequestHeadersCollection[str]);
			}
			this.EnsureRangeIsPageAligned(num, num1, str1);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			this.GetSequenceNumberConditionFromRequest(out nullable, out nullable1);
			bool flag = false;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable2, nullable, nullable1, BlobType.IndexBlob, flag, false, false, false, nullable3, null, nullable4);
			IAsyncResult asyncResult = this.serviceManager.BeginClearPage(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, num, num1 - num + (long)1, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.ClearPageImpl"));
			yield return asyncResult;
			IClearPageResult clearPageResult = this.serviceManager.EndClearPage(asyncResult);
			this.AddLastModifiedAndETagToResponse(clearPageResult.LastModifiedTime);
			this.AddSequenceNumberToResponse(clearPageResult.SequenceNumber);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> CommitBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			string str;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(this.PutBlockListTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			PutBlockListMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutBlockListMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			IAsyncResult asyncResult = null;
			byte[] numArray = null;
			byte[][] blockListFromRequest = null;
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			if (base.RequestContentLength > (long)12800000)
			{
				throw new RequestEntityTooLargeException(new long?((long)12800000));
			}
			using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(8192))
			{
				IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
				object[] objArray = new object[] { base.RequestContentLength };
				info.Log("Request content length is {0}.", objArray);
				using (Stream stream = base.GenerateMeasuredRequestStream())
				{
					asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, base.RequestContentLength, 8192, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.CommitBlobImpl"));
					yield return asyncResult;
					AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
				}
				bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
				blockListFromRequest = this.GetBlockListFromRequest(bufferPoolMemoryStream, out numArray);
			}
			operationMeasurementEvent.NumBlocks = (long)((int)blockListFromRequest.Length);
			PutBlobProperties commitBlobPropertiesFromRequest = this.GetCommitBlobPropertiesFromRequest();
			commitBlobPropertiesFromRequest.BlobType = BlobType.ListBlob;
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] blobOrFilePathName = new object[] { this.operationContext.BlobOrFilePathName, null };
			object[] objArray1 = blobOrFilePathName;
			obj = (commitBlobPropertiesFromRequest.ContentMD5 != null ? base.RequestHeadersCollection["x-ms-blob-content-md5"] : "Empty");
			objArray1[1] = obj;
			verbose.Log("UserMD5: BlobName: {0}, MD5: {1}", blobOrFilePathName);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			OverwriteOption overwriteOption = base.GetOverwriteOption(this.operationContext.RequestConditionInformation, nullable);
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable1, nullable2, nullable3, BlobType.ListBlob, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable4);
			asyncResult = this.serviceManager.BeginPutBlobFromBlocks(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, BlobServiceVersion.Mix, blockListFromRequest, null, commitBlobPropertiesFromRequest, blobObjectCondition, overwriteOption, null, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.CommitBlobImpl"));
			yield return asyncResult;
			IPutBlobFromBlocksResult putBlobFromBlocksResult = null;
			putBlobFromBlocksResult = this.serviceManager.EndPutBlobFromBlocks(asyncResult);
			base.Response.AddHeader("Content-MD5", Convert.ToBase64String(numArray));
			this.AddLastModifiedAndETagToResponse(putBlobFromBlocksResult.LastModifiedTime);
			this.AddRequestServerEncryptionStatusToResponse(putBlobFromBlocksResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> CommitBlobImplV2(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			object obj1;
			string str;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(this.PutBlockListTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			PutBlockListMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutBlockListMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			IAsyncResult asyncResult = null;
			byte[] numArray = null;
			byte[][] numArray1 = null;
			BlockSource[] blockSourceArray = null;
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			if (base.RequestContentLength > (long)12800000)
			{
				throw new RequestEntityTooLargeException(new long?((long)12800000));
			}
			using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(8192))
			{
				IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
				object[] objArray = new object[] { base.RequestContentLength };
				info.Log("Request content length is {0}.", objArray);
				using (Stream stream = base.GenerateMeasuredRequestStream())
				{
					asyncResult = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, base.RequestContentLength, 8192, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.CommitBlobImpl"));
					yield return asyncResult;
					AsyncStreamCopy.EndAsyncStreamCopy(asyncResult);
				}
				bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
				this.GetBlockListFromRequestV2(bufferPoolMemoryStream, out numArray1, out blockSourceArray, out numArray);
			}
			operationMeasurementEvent.NumBlocks = (long)((int)numArray1.Length);
			PutBlobProperties commitBlobPropertiesFromRequest = this.GetCommitBlobPropertiesFromRequest();
			commitBlobPropertiesFromRequest.BlobType = BlobType.ListBlob;
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] blobOrFilePathName = new object[] { this.operationContext.BlobOrFilePathName, null, null };
			object[] objArray1 = blobOrFilePathName;
			obj = (commitBlobPropertiesFromRequest.ContentMD5 != null ? base.RequestHeadersCollection["x-ms-blob-content-md5"] : "Empty");
			objArray1[1] = obj;
			object[] objArray2 = blobOrFilePathName;
			obj1 = (commitBlobPropertiesFromRequest.ContentCrc64.HasValue ? base.RequestHeadersCollection["x-ms-content-crc64"] : "Empty");
			objArray2[2] = obj1;
			verbose.Log("UserMD5andCRC64: BlobName: {0}, MD5: {1}, CRC64: {2}", blobOrFilePathName);
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				this.EnsureBlobTypeNotPresentOrCorrect(null);
				if (!string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-blob-sequence-number"]))
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-sequence-number", base.RequestHeadersCollection["x-ms-blob-sequence-number"]);
				}
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			OverwriteOption overwriteOption = base.GetOverwriteOption(this.operationContext.RequestConditionInformation, nullable);
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable1, nullable2, nullable3, BlobType.ListBlob, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable4);
			IPutBlobFromBlocksResult putBlobFromBlocksResult = null;
			asyncResult = this.serviceManager.BeginPutBlobFromBlocks(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, BlobServiceVersion.July09, numArray1, blockSourceArray, commitBlobPropertiesFromRequest, blobObjectCondition, overwriteOption, null, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.CommitBlobImpl"));
			yield return asyncResult;
			putBlobFromBlocksResult = this.serviceManager.EndPutBlobFromBlocks(asyncResult);
			if (base.SupportCrc64 && commitBlobPropertiesFromRequest.ContentCrc64.HasValue)
			{
				base.Response.AddHeader("x-ms-content-crc64", commitBlobPropertiesFromRequest.ContentCrc64.ToBase64String());
			}
			base.Response.AddHeader("Content-MD5", Convert.ToBase64String(numArray));
			this.AddLastModifiedAndETagToResponse(putBlobFromBlocksResult.LastModifiedTime);
			this.AddRequestServerEncryptionStatusToResponse(putBlobFromBlocksResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private string ContinuationTokenToString(string[] continuationTokenParts)
		{
			if (continuationTokenParts == null)
			{
				return "<null>";
			}
			if ((int)continuationTokenParts.Length <= 0)
			{
				return "<empty>";
			}
			int num = 0;
			string str = "[";
			string[] strArrays = continuationTokenParts;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				str = string.Concat(str, strArrays[i]);
				if (num < (int)continuationTokenParts.Length - 1)
				{
					str = string.Concat(str, ", ");
				}
				num++;
			}
			str = string.Concat(str, "]");
			return str;
		}

		private IEnumerator<IAsyncResult> CopyBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			string str;
			IAsyncResult asyncResult;
			Logger<IRestProtocolHeadLogger>.Instance.Info.Log("CopyBlobImpl");
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(HttpRestProcessor.DefaultMaxAllowedTimeoutForCopyBlob);
			this.EnsureIsRootBlobRequest();
			Guid? leaseId = this.GetLeaseId(false);
			Guid? nullable = null;
			if (!base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId("x-ms-source-lease-id", false);
			}
			DateTime? nullable1 = null;
			CopyBlobMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as CopyBlobMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			NephosAssertionException.Assert(this.operationContext.ResourceIsBlobOrFilePath);
			NephosAssertionException.Assert(!string.IsNullOrEmpty(base.RequestCopySource));
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			CopyBlobProperties copyBlobPropertiesFromRequest = this.GetCopyBlobPropertiesFromRequest();
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.CopyOperation, base.RequestRestVersion);
			OverwriteOption overwriteOption = base.GetOverwriteOption(this.operationContext.RequestConditionInformation, leaseId);
			UriString uriString = new UriString(base.RequestCopySource);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			verbose.Log("RequestCopySource is: {0}", new object[] { uriString.SafeStringForLogging });
			NephosUriComponents copySourceResourceComponents = null;
			bool flag = true;
			bool flag1 = false;
			bool flag2 = false;
			FECopyType fECopyType = FECopyType.Default;
			Uri uri = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				if (uriString.RawString.Length > 2048)
				{
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
				}
				try
				{
					uri = new Uri(uriString.RawString);
				}
				catch (UriFormatException uriFormatException)
				{
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging, uriFormatException);
				}
				if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
				{
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Copy source scheme is not http[s].");
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
				}
				try
				{
					MetadataEncoding.Validate(uriString.RawString);
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] message = new object[] { formatException.Message };
					throw new MetadataFormatException(string.Format(invariantCulture, "Invalid Copy-source contains non-ASCII character, Exception:{0}", message), formatException);
				}
				flag1 = this.IsRequestUrlSignedAccess(uri);
				flag2 = this.IsRequestUrlSignedAccess(base.RequestContext.RequestUrl);
				bool isUriPathStyle = base.IsUriPathStyle;
				bool flag3 = true;
				IAsyncResult asyncResult1 = base.BeginGetUriComponents(uri, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), async.GetResumeCallback(), async.GetResumeState("ServiceManager.BeginGetUriComponents"));
				yield return asyncResult1;
				try
				{
					copySourceResourceComponents = base.EndGetUriComponents(asyncResult1);
					NephosAssertionException.Assert(copySourceResourceComponents != null, "GetSourceUriComponents completed but returned null");
				}
				catch (InvalidUrlProtocolException invalidUrlProtocolException1)
				{
					InvalidUrlProtocolException invalidUrlProtocolException = invalidUrlProtocolException1;
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					stringDataEventStream.Log("BeginGetUriComponents on copy source failed with {0}. The source is not an Azure blob.", new object[] { invalidUrlProtocolException.Message });
					flag3 = false;
				}
				if (!flag3)
				{
					flag = false;
				}
			}
			else
			{
				copySourceResourceComponents = HttpRequestAccessor.GetCopySourceResourceComponents(uriString.RawString);
				if (copySourceResourceComponents == null)
				{
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
				}
				if (copySourceResourceComponents.AccountName != this.operationContext.AccountName)
				{
					throw new CannotCopyAcrossAccountsException(copySourceResourceComponents.AccountName, this.operationContext.AccountName);
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12 && !uri.IsLoopback)
			{
				flag = false;
			}
			if (flag)
			{
				if (base.RequestContext.IsRequestVersionAtLeastJuly09 && !string.IsNullOrEmpty(copySourceResourceComponents.AccountName) && !string.IsNullOrEmpty(copySourceResourceComponents.ContainerName) && string.IsNullOrEmpty(copySourceResourceComponents.RemainingPart))
				{
					copySourceResourceComponents.RemainingPart = copySourceResourceComponents.ContainerName;
					copySourceResourceComponents.ContainerName = "$root";
				}
				if (string.IsNullOrEmpty(copySourceResourceComponents.AccountName) || string.IsNullOrEmpty(copySourceResourceComponents.ContainerName) || string.IsNullOrEmpty(copySourceResourceComponents.RemainingPart))
				{
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
				}
				if (!base.RequestContext.IsRequestVersionAtLeastJuly09 && copySourceResourceComponents.ContainerName.Equals("$root"))
				{
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Root container is only allowed for July09+ versions.");
					throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
				}
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					string item = null;
					if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
					{
						item = HttpUtilities.GetQueryParameters(uri)["snapshot"];
					}
					else
					{
						string str1 = "?snapshot=";
						int num = copySourceResourceComponents.RemainingPart.IndexOf(str1, StringComparison.OrdinalIgnoreCase);
						if (num != -1)
						{
							string remainingPart = copySourceResourceComponents.RemainingPart;
							item = copySourceResourceComponents.RemainingPart.Substring(num + str1.Length);
							copySourceResourceComponents.RemainingPart = copySourceResourceComponents.RemainingPart.Substring(0, num);
							IStringDataEventStream verbose1 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] objArray = new object[] { remainingPart, copySourceResourceComponents.RemainingPart };
							verbose1.Log("The old remaining part was '{0}' and was trimmed to '{1}'.", objArray);
						}
					}
					if (item != null)
					{
						if (!HttpUtilities.TryGetSnapshotDateTimeFromHttpString(item, out nullable1))
						{
							IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] safeStringForLogging = new object[] { item, uriString.SafeStringForLogging };
							stringDataEventStream1.Log("The copy source is invalid because the snapshot specified in the copy source is invalid. The snapshot string was '{0}' and the copy source is '{1}'.", safeStringForLogging);
							throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
						}
						IStringDataEventStream verbose2 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						verbose2.Log("Snapshot present in copy source: '{0}'.", new object[] { item });
					}
				}
			}
			operationMeasurementEvent.SourceUrl = uriString.RawString;
			operationMeasurementEvent.DestinationAccountName = this.operationContext.AccountName;
			operationMeasurementEvent.DestinationContainerName = this.operationContext.ContainerName;
			operationMeasurementEvent.DestinationBlobName = this.operationContext.BlobOrFilePathName;
			bool flag4 = false;
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable2 = leaseId;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			DateTime? nullable5 = null;
			Guid? nullable6 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable2, nullable3, nullable4, BlobType.None, flag4, false, false, false, nullable5, null, nullable6);
			HttpRestProcessor httpRestProcessor1 = this;
			ConditionInformation conditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable7 = nullable;
			ComparisonOperator? nullable8 = null;
			long? nullable9 = null;
			blobType = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.None : BlobType.ListBlob);
			BlobObjectCondition sourceBlobObjectCondition = httpRestProcessor1.ConvertToSourceBlobObjectCondition(conditionInformation, nullable7, nullable8, nullable9, blobType, flag4, false, false, false);
			ICopyBlobResult copyBlobResult = null;
			if (flag)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("This is a SynchronousCopyBlob");
				asyncResult = this.serviceManager.BeginSynchronousCopyBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, copySourceResourceComponents.AccountName, copySourceResourceComponents.ContainerName, copySourceResourceComponents.RemainingPart, uriString, flag1, flag2, copyBlobPropertiesFromRequest, blobObjectCondition, overwriteOption, sourceBlobObjectCondition, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("this.serviceManager.SynchronousCopyBlob"));
				yield return asyncResult;
				copyBlobResult = this.serviceManager.EndSynchronousCopyBlob(asyncResult);
			}
			if (!flag)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("This is an AsynchronousCopyBlob");
				bool flag5 = false;
				if (base.RequestContext.IsRequestVersionAtLeastMay16)
				{
					flag5 = true;
				}
				bool flag6 = false;
				fECopyType = (fECopyType == FECopyType.Default ? FECopyType.Async : fECopyType);
				asyncResult = this.serviceManager.BeginAsynchronousCopyBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, uriString, flag5, flag6, fECopyType, nullable1, copyBlobPropertiesFromRequest, blobObjectCondition, overwriteOption, sourceBlobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("this.serviceManager.AsynchronousCopyBlob"));
				yield return asyncResult;
				TimeSpan zero = TimeSpan.Zero;
				try
				{
					copyBlobResult = this.serviceManager.EndAsynchronousCopyBlob(asyncResult, out zero);
				}
				finally
				{
					T timeSpan = this.operationContext;
					TimeSpan operationInternetRequestRoundTripLatency = this.operationContext.OperationInternetRequestRoundTripLatency;
					timeSpan.OperationInternetRequestRoundTripLatency = new TimeSpan(operationInternetRequestRoundTripLatency.Ticks + zero.Ticks);
				}
			}
			this.AddLastModifiedAndETagToResponse(copyBlobResult.LastModifiedTime);
			if (!base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			}
			else
			{
				this.AddCopyInfoToResponse(copyBlobResult.CopyInfo);
				this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			}
			base.SendSuccessResponse(false);
		}

		public static IProcessor Create(Microsoft.Cis.Services.Nephos.Common.RequestContext requestContext, IStorageManager storageManager, Microsoft.Cis.Services.Nephos.Common.Authentication.AuthenticationManager authenticationManager, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager, HttpProcessorConfiguration configuration, TransformExceptionDelegate transformProviderException, IIpThrottlingTable ipThrottlingTable)
		{
			return new HttpRestProcessor(requestContext, storageManager, authenticationManager, serviceManager, configuration, transformProviderException, ipThrottlingTable);
		}

		private IEnumerator<IAsyncResult> CreateContainerImpl(AsyncIteratorContext<NoResults> async)
		{
			ContainerAclSettings containerAclSetting;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			base.CreateContainerAclSettingsFromHeader(false, out containerAclSetting);
			IAsyncResult asyncResult = this.serviceManager.BeginCreateContainer(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nameValueCollection, containerAclSetting, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.CreateContainerImpl"));
			yield return asyncResult;
			this.AddLastModifiedAndETagToResponse(this.serviceManager.EndCreateContainer(asyncResult));
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private byte[][] DecodeBlockListFromStream(Stream inputStream)
		{
			byte[][] array;
			try
			{
				using (XmlReader xmlReader = XmlReader.Create(inputStream, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.DefaultXmlReaderSettings))
				{
					IXmlLineInfo xmlLineInfo = xmlReader as IXmlLineInfo;
					xmlReader.Read();
					xmlReader.ReadStartElement("BlockList");
					List<byte[]> numArrays = new List<byte[]>();
					while (xmlReader.IsStartElement("Block"))
					{
						string str = xmlReader.ReadElementString("Block");
						if (string.IsNullOrEmpty(str))
						{
							throw new InvalidXmlProtocolException("Block ID cannot be empty", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						byte[] numArray = null;
						try
						{
							numArray = Convert.FromBase64String(str);
							IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
							object[] objArray = new object[] { str, Encoding.UTF8.GetString(numArray) };
							verboseDebug.Log("Processing blockIdBase64: {0} blockId: {1}", objArray);
						}
						catch (FormatException formatException1)
						{
							FormatException formatException = formatException1;
							Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Processing blockIdBase64: {0}", new object[] { str });
							throw new InvalidBlockIdProtocolException(str, xmlLineInfo, formatException);
						}
						numArrays.Add(numArray);
					}
					if (numArrays.Count > 0)
					{
						xmlReader.ReadEndElement();
					}
					if (xmlReader.Read())
					{
						throw new InvalidXmlProtocolException("Error parsing Xml content. There are nodes beyond the root element BlockList");
					}
					array = numArrays.ToArray();
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException("Error parsing Xml content", xmlException.LineNumber, xmlException.LinePosition);
			}
			return array;
		}

		private void DecodeBlockListFromStreamV2(Stream inputStream, out byte[][] blockIdList, out BlockSource[] blockSourceList)
		{
			string str;
			blockIdList = null;
			blockSourceList = null;
			try
			{
				using (XmlReader xmlReader = XmlReader.Create(inputStream, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.DefaultXmlReaderSettings))
				{
					List<byte[]> numArrays = new List<byte[]>();
					List<BlockSource> blockSources = new List<BlockSource>();
					BlockSource blockSource = BlockSource.None;
					IXmlLineInfo xmlLineInfo = xmlReader as IXmlLineInfo;
					xmlReader.Read();
					xmlReader.ReadStartElement("BlockList");
					while (xmlReader.IsStartElement())
					{
						if (xmlReader.Name.Equals("Uncommitted"))
						{
							blockSource = BlockSource.Uncommitted;
						}
						else if (!xmlReader.Name.Equals("Committed"))
						{
							if (!xmlReader.Name.Equals("Latest"))
							{
								throw new InvalidXmlProtocolException("Error parsing Xml content", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
							}
							blockSource = BlockSource.Latest;
						}
						else
						{
							blockSource = BlockSource.Committed;
						}
						byte[] numArray = null;
						xmlReader.ReadStartElement();
						try
						{
							str = xmlReader.ReadContentAsString();
						}
						catch (InvalidOperationException invalidOperationException)
						{
							throw new InvalidXmlProtocolException("Error reading the block ID", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						if (string.IsNullOrEmpty(str))
						{
							throw new InvalidXmlProtocolException("Block ID cannot be empty", xmlLineInfo.LineNumber, xmlLineInfo.LinePosition);
						}
						try
						{
							numArray = Convert.FromBase64String(str);
							IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
							object[] objArray = new object[] { str, Encoding.UTF8.GetString(numArray) };
							verboseDebug.Log("Processing blockIdBase64: {0} blockId: {1}", objArray);
						}
						catch (FormatException formatException1)
						{
							FormatException formatException = formatException1;
							Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log("Processing blockIdBase64: {0}", new object[] { str });
							throw new InvalidBlockIdProtocolException(str, xmlLineInfo, formatException);
						}
						xmlReader.ReadEndElement();
						numArrays.Add(numArray);
						blockSources.Add(blockSource);
					}
					if (numArrays.Count > 0)
					{
						xmlReader.ReadEndElement();
					}
					if (xmlReader.Read())
					{
						throw new InvalidXmlProtocolException("Error parsing Xml content. There are nodes beyond the root element BlockList");
					}
					blockIdList = numArrays.ToArray();
					blockSourceList = blockSources.ToArray();
				}
			}
			catch (XmlException xmlException1)
			{
				XmlException xmlException = xmlException1;
				throw new InvalidXmlProtocolException("Error parsing Xml content", xmlException.LineNumber, xmlException.LinePosition);
			}
		}

		private IEnumerator<IAsyncResult> DeleteBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			DateTime? nullable = this.GetSnapshot(false, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			if (nullable.HasValue && !base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				throw new InvalidQueryParameterProtocolException("snapshot", HttpUtilities.ConvertSnapshotDateTimeToHttpString(nullable.Value), string.Format("Snapshots are only supported for versions {0} and up.", "2009-09-19"));
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			bool flag = true;
			bool flag1 = false;
			bool isRequestVersionAtLeastAugust13 = base.RequestContext.IsRequestVersionAtLeastAugust13;
			string item = null;
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				item = base.RequestHeadersCollection["x-ms-delete-snapshots"];
			}
			if (!string.IsNullOrEmpty(item))
			{
				this.EnsureIsRootBlobRequest();
				if (!item.Equals("only", StringComparison.OrdinalIgnoreCase))
				{
					if (!item.Equals("include", StringComparison.OrdinalIgnoreCase))
					{
						throw new InvalidHeaderProtocolException("x-ms-delete-snapshots", item);
					}
					flag1 = false;
					flag = false;
				}
				else
				{
					flag1 = true;
					flag = false;
				}
			}
			Guid? nullable1 = this.GetLeaseId(false);
			bool flag2 = false;
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable2 = nullable1;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			blobType = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.None : BlobType.ListBlob);
			DateTime? nullable5 = null;
			Guid? nullable6 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable2, nullable3, nullable4, blobType, flag2, false, true, false, nullable5, null, nullable6) ?? new BlobObjectCondition();
			blobObjectCondition.IsRequiringNoSnapshots = flag;
			blobObjectCondition.IsDeletingOnlySnapshots = flag1;
			blobObjectCondition.IsIncludingUncommittedBlobs = isRequestVersionAtLeastAugust13;
			IAsyncResult asyncResult = this.serviceManager.BeginDeleteBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, blobObjectCondition, nullable, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.DeleteBlobImpl"));
			yield return asyncResult;
			this.serviceManager.EndDeleteBlob(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> DeleteContainerImpl(AsyncIteratorContext<NoResults> async)
		{
			DateTime? ifModifiedSince;
			DateTime? ifNotModifiedSince;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId(false);
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
			IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
			string accountName = this.operationContext.AccountName;
			string containerName = this.operationContext.ContainerName;
			Guid? nullable1 = nullable;
			if (this.operationContext.RequestConditionInformation != null)
			{
				ifModifiedSince = this.operationContext.RequestConditionInformation.IfModifiedSince;
			}
			else
			{
				ifModifiedSince = null;
			}
			if (this.operationContext.RequestConditionInformation != null)
			{
				ifNotModifiedSince = this.operationContext.RequestConditionInformation.IfNotModifiedSince;
			}
			else
			{
				ifNotModifiedSince = null;
			}
			DateTime? nullable2 = null;
			bool? nullable3 = null;
			bool? nullable4 = null;
			IAsyncResult asyncResult = serviceManager.BeginDeleteContainer(callerIdentity, accountName, containerName, nullable1, ifModifiedSince, ifNotModifiedSince, nullable2, nullable3, nullable4, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.DeleteContainerImpl"));
			yield return asyncResult;
			this.serviceManager.EndDeleteContainer(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private void EnsureBlobTypeNotPresentOrCorrect(string onlyAllowedBlobType)
		{
			string item = base.RequestHeadersCollection["x-ms-blob-type"];
			if (!string.IsNullOrEmpty(item) && (string.IsNullOrEmpty(onlyAllowedBlobType) || !item.Equals(onlyAllowedBlobType, StringComparison.OrdinalIgnoreCase)))
			{
				throw new InvalidHeaderProtocolException("x-ms-blob-type", item);
			}
		}

		public void EnsureCrc64AndMd5HeaderAreMutuallyExclusive(bool useMsMd5Header)
		{
			string str = (useMsMd5Header ? base.RequestHeadersCollection["x-ms-blob-content-md5"] : base.RequestHeadersCollection["Content-MD5"]);
			if (base.SupportCrc64 && base.RequestHeadersCollection["x-ms-content-crc64"] != null && str != null)
			{
				if (!useMsMd5Header)
				{
					throw new BothCrc64AndMd5HeaderPresentException();
				}
				throw new BothCrc64AndMsMd5HeaderPresentException();
			}
		}

		public void EnsureCrc64AndMd5RangeHeaderAreMutuallyExclusive()
		{
			if (base.SupportCrc64 && base.RequestHeadersCollection["x-ms-range-get-content-crc64"] != null && base.RequestHeadersCollection["x-ms-range-get-content-md5"] != null)
			{
				throw new BothCrc64AndMd5RangeHeaderPresentException();
			}
		}

		protected void EnsureIsRootBlobRequest()
		{
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				return;
			}
			this.GetSnapshot(true, true, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
		}

		private void EnsureIsUsingHttps(string commandName)
		{
			if (!base.IsUsingHttps)
			{
				throw new HttpsRequiredProtocolException(commandName, null);
			}
		}

		private void EnsureRangeIsPageAligned(long startOffset, long endOffset, string headerValue)
		{
			long num = endOffset - startOffset + (long)1;
			if (startOffset % (long)512 != (long)0)
			{
				throw new InvalidHeaderProtocolException("Range", headerValue);
			}
			if (num % (long)512 != (long)0)
			{
				throw new InvalidHeaderProtocolException("Range", headerValue);
			}
		}

		protected void EnsureRequestHttpVersionIsSupported(List<Version> unsupportedHttpVersions)
		{
			if (unsupportedHttpVersions != null && unsupportedHttpVersions.Contains(base.RequestProtocolVersion))
			{
				throw new HttpVersionNotSupportedException(base.RequestProtocolVersion, base.RequestVia);
			}
		}

		protected override OperationContextWithAuthAndAccountContainer ExtractOperationContext(NephosUriComponents uriComponents)
		{
			if (string.IsNullOrEmpty(uriComponents.AccountName))
			{
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			OperationContext operationContext = new OperationContext(base.RequestContext.ElapsedTime);
			string item = base.RequestQueryParameters["restype"];
			operationContext.IsResourceTypeContainer = false;
			operationContext.IsResourceTypeService = false;
			operationContext.IsResourceTypeDirectory = false;
			if (!string.IsNullOrEmpty(item))
			{
				if (base.RequestContext.ServiceType == Microsoft.Cis.Services.Nephos.Common.ServiceType.FileService && "share".Equals(item, StringComparison.Ordinal))
				{
					operationContext.IsResourceTypeContainer = true;
				}
				else if (base.RequestContext.ServiceType != Microsoft.Cis.Services.Nephos.Common.ServiceType.FileService && "container".Equals(item, StringComparison.Ordinal))
				{
					operationContext.IsResourceTypeContainer = true;
				}
				else if ("service".Equals(item, StringComparison.Ordinal))
				{
					operationContext.IsResourceTypeService = true;
				}
				else if ("directory".Equals(item, StringComparison.Ordinal))
				{
					operationContext.IsResourceTypeDirectory = true;
				}
			}
			operationContext.SubResource = base.RequestQueryParameters["comp"];
			this.ValidateAndAdjustContext(operationContext, uriComponents, false);
			operationContext.SetUserTimeout(base.ExtractTimeoutFromContext());
			operationContext.IsUnversionedRequest = (!string.IsNullOrEmpty(base.RequestHeaderRestVersion) ? false : string.IsNullOrEmpty(base.ContainerAclAdjustedRequestRestVersion));
			operationContext.IsRootContainerImplicit = uriComponents.IsRootContainerImplicit;
			return operationContext;
		}

		protected void FillListBaseObjectsOperationContext(ListBaseObjectsOperationContext loc)
		{
			this.FillListOperationContext(loc);
			bool supportSurrogatesInListingPrefixString = loc.SupportSurrogatesInListingPrefixString;
			loc.Delimiter = base.RequestQueryParameters["delimiter"];
			if (XmlUtilities.HasInvalidXmlCharacters(loc.Delimiter, supportSurrogatesInListingPrefixString))
			{
				throw new InvalidQueryParameterProtocolException("delimiter", XmlUtilities.EscapeInvalidXmlCharacters(loc.Delimiter, false), "The string includes invalid Xml Characters");
			}
			loc.IncludeIfModifiedSince = base.ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "modifiedsince");
			loc.IncludeIfNotModifiedSince = base.ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "notmodifiedsince");
			bool? nullable = base.ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "includedisabledcontainers");
			if (nullable.HasValue)
			{
				loc.IncludeDisabledContainers = nullable.Value;
			}
			loc.IsUsingPropertiesElement = base.RequestContext.IsRequestVersionAtLeastSeptember09;
			if (base.RequestContext.ServiceType != Microsoft.Cis.Services.Nephos.Common.ServiceType.BlobService || !base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				loc.IsIncludingLeaseStateAndDurationInResponse = false;
				loc.IsIncludingLeaseStatusInResponse = false;
			}
			else
			{
				loc.IsIncludingLeaseStateAndDurationInResponse = true;
				loc.IsIncludingLeaseStatusInResponse = true;
			}
			loc.IsIncludingPublicAccessInResponse = base.RequestContext.IsRequestVersionAtLeastMay16;
		}

		protected void FillListBlobsOperationContext(ListBlobsOperationContext blobsListingContext, bool listingAcrossContainers)
		{
			this.FillListBaseObjectsOperationContext(blobsListingContext);
			blobsListingContext.ListingAcrossContainers = listingAcrossContainers;
			string[] strArrays = null;
			ContinuationTokenVersion continuationTokenVersion = ContinuationTokenVersion.None;
			if (!string.IsNullOrEmpty(blobsListingContext.Marker))
			{
				try
				{
					strArrays = ContinuationTokenParser.DecodeContinuationToken(blobsListingContext.Marker, HttpRestProcessor.ValidListBlobsContinuationTokenVersions, out continuationTokenVersion);
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] str = new object[] { this.ContinuationTokenToString(strArrays), continuationTokenVersion, blobsListingContext.ListingAcrossContainers, blobsListingContext.Marker };
					verbose.Log("FillListBlobsOperationContext >> ContinuationToken = {0}, ContinuationTokenVersion = {1}, ListingAcrossContainers = {2}, Marker = '{3}'.", str);
				}
				catch (ArgumentException argumentException)
				{
					throw new InvalidMarkerException("InvalidFormat", argumentException);
				}
			}
			if (strArrays != null && (int)strArrays.Length > 0)
			{
				if (continuationTokenVersion != ContinuationTokenVersion.VersionOne)
				{
					if (continuationTokenVersion != ContinuationTokenVersion.VersionTwo)
					{
						Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Parsing a continuation token for ListBlobs encountered an invalid continuation token version {0}.", new object[] { continuationTokenVersion });
						throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, string.Format("Invalid ListBlobs marker.", new object[0]));
					}
					if (!listingAcrossContainers)
					{
						if ((int)strArrays.Length > 2)
						{
							throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, "Listing within a container did not specify a marker that could be parsed into blob and snapshot markers.");
						}
						DateTime? nullable = null;
						if ((int)strArrays.Length == 2 && !HttpUtilities.TryGetSnapshotDateTimeFromHttpString(strArrays[1], out nullable))
						{
							throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, "Listing within a container encountered an invalid snapshot marker.");
						}
						blobsListingContext.SnapshotMarker = nullable;
						blobsListingContext.Marker = strArrays[0];
					}
					else
					{
						if ((int)strArrays.Length < 2 || (int)strArrays.Length > 3)
						{
							throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, "Listing across containers did not specify a marker that could be parsed into container, blob and snapshot markers.");
						}
						DateTime? nullable1 = null;
						if ((int)strArrays.Length == 3 && !HttpUtilities.TryGetSnapshotDateTimeFromHttpString(strArrays[2], out nullable1))
						{
							throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, "Listing within a container encountered an invalid snapshot marker.");
						}
						blobsListingContext.SnapshotMarker = nullable1;
						blobsListingContext.ContainerMarkerListingBlobs = strArrays[0];
						blobsListingContext.Marker = strArrays[1];
					}
				}
				else
				{
					NephosAssertionException.Assert((int)strArrays.Length == 1, "V1 of continuation tokens should always have only one part.");
					if (!listingAcrossContainers)
					{
						blobsListingContext.Marker = strArrays[0];
					}
					else
					{
						string str1 = strArrays[0];
						string[] strArrays1 = new string[] { "/" };
						string[] strArrays2 = str1.Split(strArrays1, 2, StringSplitOptions.None);
						if ((int)strArrays2.Length != 2)
						{
							throw new InvalidQueryParameterProtocolException("marker", blobsListingContext.Marker, "Listing across containers did not specify a marker that could be parsed into container and blob markers.");
						}
						blobsListingContext.ContainerMarkerListingBlobs = strArrays2[0];
						blobsListingContext.Marker = strArrays2[1];
					}
				}
			}
			blobsListingContext.IsIncludingCacheControlInResponse = base.RequestContext.IsRequestVersionAtLeastSeptember09;
			blobsListingContext.IsFetchingMetadata = false;
			blobsListingContext.IsIncludingContentDispositionInResponse = base.RequestContext.IsRequestVersionAtLeastAugust13;
			blobsListingContext.IsIncludingLeaseStatusInResponse = base.RequestContext.IsRequestVersionAtLeastSeptember09;
			blobsListingContext.IsIncludingBlobTypeInResponse = base.RequestContext.IsRequestVersionAtLeastSeptember09;
			blobsListingContext.IsIncludingPageBlobs = base.RequestContext.IsRequestVersionAtLeastSeptember09;
			blobsListingContext.IsIncludingAppendBlobs = base.RequestContext.IsRequestVersionAtLeastFebruary15;
			blobsListingContext.IsIncludingEncryption = base.RequestContext.IsRequestVersionAtLeastDecember15;
			blobsListingContext.IsIncludingIncrementalCopy = base.RequestContext.IsRequestVersionAtLeastMay16;
			blobsListingContext.IsIncludingSnapshots = false;
			blobsListingContext.IsIncludingUncommittedBlobs = false;
			blobsListingContext.IsIncludingCopyPropertiesInResponse = false;
			blobsListingContext.IsIncludingCrc64InResponse = base.SupportCrc64;
			blobsListingContext.IsIncludingAccessTierInResponse = base.RequestContext.IsRequestVersionAtLeastApril17;
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && !blobsListingContext.ListingAcrossContainers)
			{
				List<string> strs = base.ParseOptionalListInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "include", HttpRestProcessor.ValidListBlobsIncludeQueryParamValues, ",");
				if (strs != null && strs.Count > 0)
				{
					foreach (string str2 in strs)
					{
						if (str2.Equals("metadata", StringComparison.OrdinalIgnoreCase))
						{
							blobsListingContext.IsFetchingMetadata = true;
						}
						else if (str2.Equals("snapshots", StringComparison.OrdinalIgnoreCase))
						{
							if (!string.IsNullOrEmpty(blobsListingContext.Delimiter))
							{
								throw new InvalidQueryParameterProtocolException("include", str2, "Listing snapshots and using a delimiter are mutually exclusive.");
							}
							blobsListingContext.IsIncludingSnapshots = true;
						}
						else if (str2.Equals("uncommittedblobs", StringComparison.OrdinalIgnoreCase))
						{
							blobsListingContext.IsIncludingUncommittedBlobs = true;
						}
						else if (!str2.Equals("copy", StringComparison.OrdinalIgnoreCase) || !base.RequestContext.IsRequestVersionAtLeastFebruary12)
						{
							if (str2.Equals("deleted", StringComparison.OrdinalIgnoreCase) && base.RequestContext.IsRequestVersionAtLeastApril17)
							{
								continue;
							}
							throw new InvalidQueryParameterProtocolException("include", str2, "Invalid query parameter value.");
						}
						else
						{
							blobsListingContext.IsIncludingCopyPropertiesInResponse = true;
						}
					}
				}
			}
		}

		protected void FillListOperationContext(ListOperationContext loc)
		{
			bool supportSurrogatesInListingPrefixString = loc.SupportSurrogatesInListingPrefixString;
			loc.Prefix = base.RequestQueryParameters["prefix"];
			if (XmlUtilities.HasInvalidXmlCharacters(loc.Prefix, supportSurrogatesInListingPrefixString))
			{
				throw new InvalidQueryParameterProtocolException("prefix", XmlUtilities.EscapeInvalidXmlCharacters(loc.Prefix, false), "The string includes invalid Xml Characters");
			}
			loc.Marker = base.RequestQueryParameters["marker"];
			if (XmlUtilities.HasInvalidXmlCharacters(loc.Marker, supportSurrogatesInListingPrefixString))
			{
				throw new InvalidQueryParameterProtocolException("marker", XmlUtilities.EscapeInvalidXmlCharacters(loc.Marker, false), "The string includes invalid Xml Characters");
			}
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				loc.MaxResults = base.GetMaxResultsQueryParamPreSeptember09VersionForBlob();
			}
			else
			{
				loc.MaxResults = base.GetMaxResultsQueryParam();
			}
			loc.IsIncludingUrlInResponse = !base.RequestContext.IsRequestVersionAtLeastAugust13;
		}

		private long? GetBlobAppendPositionConditionFromRequest()
		{
			return base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-blob-condition-appendpos", null);
		}

		private IEnumerator<IAsyncResult> GetBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			bool flag;
			IAsyncResult asyncResult;
			long num;
			long num1;
			this.EnsureRequestHttpVersionIsSupported(null);
			this.EnsureCrc64AndMd5RangeHeaderAreMutuallyExclusive();
			GetBlobMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as GetBlobMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			NephosAssertionException.Assert(this.operationContext.ResourceIsBlobOrFilePath);
			Guid? nullable = this.GetLeaseId(false);
			DateTime? nullable1 = this.GetSnapshot(true, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			if (!base.RequestContext.MultipleConditionalHeadersEnabled)
			{
				this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			}
			else
			{
				this.operationContext.RequestConditionInformation = MultipleConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			}
			this.Range = HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection);
			bool rangeFromRequestAndAdjustEndOffset = this.GetRangeFromRequestAndAdjustEndOffset(base.RequestHeadersCollection, base.RequestContext.IsRequestVersionAtLeastAugust11, out num, out num1, out flag);
			bool flag1 = false;
			bool flag2 = false;
			bool isCalculatingRangeCrc64FromRequest = false;
			if (base.SupportCrc64)
			{
				isCalculatingRangeCrc64FromRequest = this.GetIsCalculatingRangeCrc64FromRequest(rangeFromRequestAndAdjustEndOffset, flag, num1 - num + (long)1);
			}
			bool isCalculatingRangeMD5FromRequest = false;
			if (!isCalculatingRangeCrc64FromRequest)
			{
				isCalculatingRangeMD5FromRequest = this.GetIsCalculatingRangeMD5FromRequest(rangeFromRequestAndAdjustEndOffset, flag, num1 - num + (long)1);
			}
			flag2 = isCalculatingRangeCrc64FromRequest;
			BlobPropertyNames blobPropertyName = BlobPropertyNames.None;
			bool flag3 = this.ShouldExcludeNonSystemHeaders();
			bool flag4 = this.ShouldIncludeInternalProperties();
			Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobProperty = (IBlobProperties blobProperties) => {
				long contentLength;
				if (rangeFromRequestAndAdjustEndOffset && true && blobProperties.ContentLength == (long)0 && num >= blobProperties.ContentLength)
				{
					throw new InvalidBlobRegionException(new long?(blobProperties.ContentLength), "The starting offset is greater than the length of the blob.");
				}
				this.SetBlobPropertiesOnResponse(blobProperties, (!this.RequestContext.IsRequestVersionAtLeastSeptember09 ? true : !rangeFromRequestAndAdjustEndOffset), this.RequestContext.IsRequestVersionAtLeastAugust11, (!this.RequestContext.IsRequestVersionAtLeastMay16 ? false : rangeFromRequestAndAdjustEndOffset), flag3, flag4, false);
				if (!rangeFromRequestAndAdjustEndOffset)
				{
					contentLength = blobProperties.ContentLength;
					this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
				}
				else
				{
					this.SetStatusCodeAndServiceHeaders(HttpStatusCode.PartialContent);
					if (num1 >= blobProperties.ContentLength)
					{
						num1 = blobProperties.ContentLength - (long)1;
					}
					this.Response.ContentLength64 = num1 - num + (long)1;
					contentLength = this.Response.ContentLength64;
					this.Response.AddHeader("Content-Range", string.Format("bytes {0}-{1}/{2}", num, num1, blobProperties.ContentLength));
				}
				operationMeasurementEvent.BlobSize = contentLength;
				operationMeasurementEvent.BlobSizeKnown = true;
				operationMeasurementEvent.BlobType = blobProperties.BlobType;
				this.operationContext.MaxAllowedTimeout = BlobObjectHelper.GetSizeBasedTimeout(contentLength, TimeSpan.MaxValue, BlobObjectHelper.MinGetBlobTimeout, ServiceConstants.DefaultGetDataRateForTimeout);
				base.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
				this.operationContext.IsSizeBased = true;
				return true;
			};
			blobType = ((base.RequestHeaderRestVersion != null || base.IsRequestAuthenticated) && !base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.ListBlob : BlobType.None);
			BlobType blobType1 = blobType;
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable2 = nullable;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable2, nullable3, nullable4, blobType1, false, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable5);
			using (Stream stream = base.GenerateMeasuredResponseStream(false))
			{
				try
				{
					if (rangeFromRequestAndAdjustEndOffset)
					{
						Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeCrc64 response = (long? crc64) => {
							if (isCalculatingRangeCrc64FromRequest)
							{
								this.AddNephosContentCrc64ToResponse(crc64);
							}
						};
						Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeMD5 interceptRangeMD5 = (byte[] md5) => this.AddContentMD5ToResponse(md5, "Content-MD5");
						asyncResult = this.serviceManager.BeginGetBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, stream, num, num1 - num + (long)1, flag2, isCalculatingRangeMD5FromRequest, blobPropertyName, blobObjectCondition, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), flag3, interceptBlobProperty, response, interceptRangeMD5, base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobImpl"));
					}
					else
					{
						asyncResult = this.serviceManager.BeginGetBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, stream, blobPropertyName, blobObjectCondition, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), flag3, interceptBlobProperty, base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobImpl"));
					}
					yield return asyncResult;
					this.serviceManager.EndGetBlob(asyncResult);
				}
				finally
				{
				}
			}
			this.SendCacheSuccessResponse(flag1);
		}

		private IEnumerator<IAsyncResult> GetBlobMetadataImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = this.GetLeaseId(false);
			DateTime? nullable1 = this.GetSnapshot(true, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			if (base.RequestContext.MultipleConditionalHeadersEnabled)
			{
				this.operationContext.RequestConditionInformation = MultipleConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			}
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable2 = nullable;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			blobType = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.None : BlobType.ListBlob);
			DateTime? nullable5 = null;
			Guid? nullable6 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable2, nullable3, nullable4, blobType, false, false, true, false, nullable5, null, nullable6);
			IAsyncResult asyncResult = this.serviceManager.BeginGetBlobMetadata(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, blobObjectCondition, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobMetadataImpl"));
			yield return asyncResult;
			NameValueCollection nameValueCollection = null;
			DateTime dateTime = this.serviceManager.EndGetBlobMetadata(asyncResult, out nameValueCollection);
			if (nameValueCollection != null)
			{
				base.SetMetadataOnResponse(nameValueCollection);
			}
			this.AddLastModifiedAndETagToResponse(dateTime);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetBlobMetadataOrGetContainerMetadataImpl(AsyncIteratorContext<NoResults> async)
		{
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Doing a double-lookup for GetBlobMetadata or GetContainerMetadata. Trying GetBlobMetadata first...");
			IAsyncResult asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobMetadataImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobMetadataOrGetContainerMetadataImpl"));
			yield return asyncResult;
			bool flag = false;
			try
			{
				base.EndPerformOperation(asyncResult);
				flag = true;
			}
			catch (BlobNotFoundException blobNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = blobNotFoundException;
			}
			catch (ContainerNotFoundException containerNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerNotFoundException;
			}
			catch (ContainerUnauthorizedException containerUnauthorizedException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerUnauthorizedException;
			}
			if (!flag)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] accountName = new object[] { this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName };
				verbose.Log("Acceptable Exception for GetBlobMetadata; trying GetContainerMetadata. Account name = {0}, Container name = {1}, Blob name = {2}; will use the blob name as the container name.", accountName);
				this.operationContext.OperationMeasurementEvent = this.operationContext.TemporaryGetContainerMetadataMeasurementEvent;
				this.operationContext.ContainerName = this.operationContext.BlobOrFilePathName;
				this.operationContext.BlobOrFilePathName = null;
				asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerMetadataImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobMetadataOrGetContainerMetadataImpl"));
				yield return asyncResult;
				try
				{
					base.EndPerformOperation(asyncResult);
				}
				catch (ContainerNotFoundException containerNotFoundException1)
				{
					this.RethrowBlobErrorForDoubleLookupIfNecessary();
					throw;
				}
			}
		}

		private IEnumerator<IAsyncResult> GetBlobOrGetContainerPropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Doing a double-lookup for GetBlob or GetContainerProperties. Trying GetBlob first...");
			IAsyncResult asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobOrGetContainerPropertiesImpl"));
			yield return asyncResult;
			bool flag = false;
			try
			{
				base.EndPerformOperation(asyncResult);
				flag = true;
			}
			catch (BlobNotFoundException blobNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = blobNotFoundException;
			}
			catch (ContainerNotFoundException containerNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerNotFoundException;
			}
			catch (ContainerUnauthorizedException containerUnauthorizedException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerUnauthorizedException;
			}
			if (!flag)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] accountName = new object[] { this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName };
				verbose.Log("Acceptable Exception for GetBlob; trying GetContainerProperties. Account name = {0}, Container name = {1}, Blob name = {2}; will use the blob name as the container name.", accountName);
				this.operationContext.OperationMeasurementEvent = this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent;
				this.operationContext.ContainerName = this.operationContext.BlobOrFilePathName;
				this.operationContext.BlobOrFilePathName = null;
				asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerPropertiesImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobOrGetContainerPropertiesImpl"));
				yield return asyncResult;
				try
				{
					base.EndPerformOperation(asyncResult);
				}
				catch (ContainerNotFoundException containerNotFoundException1)
				{
					this.RethrowBlobErrorForDoubleLookupIfNecessary();
					throw;
				}
			}
		}

		private IEnumerator<IAsyncResult> GetBlobPropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = this.GetLeaseId(false);
			DateTime? nullable1 = this.GetSnapshot(true, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			if (!base.RequestContext.MultipleConditionalHeadersEnabled)
			{
				this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			}
			else
			{
				this.operationContext.RequestConditionInformation = MultipleConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			}
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable2 = nullable;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			blobType = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.None : BlobType.ListBlob);
			DateTime? nullable5 = null;
			Guid? nullable6 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable2, nullable3, nullable4, blobType, false, false, true, false, nullable5, null, nullable6);
			bool flag = this.ShouldExcludeNonSystemHeaders();
			bool flag1 = this.ShouldIncludeInternalProperties();
			IAsyncResult asyncResult = this.serviceManager.BeginGetBlobProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, base.SupportCrc64, blobObjectCondition, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), flag, base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobPropertiesImpl"));
			yield return asyncResult;
			IBlobProperties blobProperty = this.serviceManager.EndGetBlobProperties(asyncResult);
			NephosAssertionException.Assert(blobProperty != null);
			bool flag2 = false;
			this.SetBlobPropertiesOnResponse(blobProperty, true, base.RequestContext.IsRequestVersionAtLeastAugust13, false, flag, flag1, flag2);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetBlobPropertiesOrGetContainerPropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Doing a double-lookup for GetBlobProperties or GetContainerProperties. Trying GetBlobProperties first...");
			IAsyncResult asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetBlobPropertiesImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobPropertiesOrGetContainerPropertiesImpl"));
			yield return asyncResult;
			bool flag = false;
			try
			{
				base.EndPerformOperation(asyncResult);
				flag = true;
			}
			catch (BlobNotFoundException blobNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = blobNotFoundException;
			}
			catch (ContainerNotFoundException containerNotFoundException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerNotFoundException;
			}
			catch (ContainerUnauthorizedException containerUnauthorizedException)
			{
				this.operationContext.BlobErrorToRethrowForDoubleLookup = containerUnauthorizedException;
			}
			if (!flag)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] accountName = new object[] { this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName };
				verbose.Log("Acceptable Exception for GetBlobProperties; trying GetContainerProperties. Account name = {0}, Container name = {1}, Blob name = {2}; will use the blob name as the container name.", accountName);
				this.operationContext.OperationMeasurementEvent = this.operationContext.TemporaryGetContainerPropertiesMeasurementEvent;
				this.operationContext.ContainerName = this.operationContext.BlobOrFilePathName;
				this.operationContext.BlobOrFilePathName = null;
				asyncResult = base.BeginPerformOperation(new BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.RestMethodImpl(this.GetContainerPropertiesImpl), async.GetResumeCallback(), async.GetResumeState("GetBlobPropertiesOrGetContainerPropertiesImpl"));
				yield return asyncResult;
				try
				{
					base.EndPerformOperation(asyncResult);
				}
				catch (ContainerNotFoundException containerNotFoundException1)
				{
					this.RethrowBlobErrorForDoubleLookupIfNecessary();
					throw;
				}
			}
		}

		private IEnumerator<IAsyncResult> GetBlobServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			IAsyncResult asyncResult = this.serviceManager.BeginGetBlobServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = this.serviceManager.EndGetBlobServiceProperties(asyncResult);
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteAnalyticsSettings(analyticsSetting, AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobServicePropertiesImpl"));
			yield return asyncResult;
			base.EndWriteAnalyticsSettings(asyncResult);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetBlobServiceStatsImpl(AsyncIteratorContext<NoResults> async)
		{
			object obj;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			if (!base.UriComponents.IsSecondaryAccountAccess)
			{
				throw new InvalidQueryParameterProtocolException("comp", this.operationContext.SubResource, null);
			}
			IAsyncResult asyncResult = this.serviceManager.BeginGetBlobServiceStats(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobServiceStatsImpl"));
			yield return asyncResult;
			GeoReplicationStats geoReplicationStat = this.serviceManager.EndGetBlobServiceStats(asyncResult);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] blobOrFilePathName = new object[] { this.operationContext.BlobOrFilePathName, null };
			object[] objArray = blobOrFilePathName;
			obj = (geoReplicationStat != null ? geoReplicationStat.ToString() : "null");
			objArray[1] = obj;
			verbose.Log("Blob {0} GeoReplicationStats is '{1}'", blobOrFilePathName);
			base.AddServiceResponseHeadersBeforeSendingResponse();
			asyncResult = base.BeginWriteServiceStats(geoReplicationStat, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlobServiceStatsImpl"));
			yield return asyncResult;
			base.EndWriteServiceStats(asyncResult);
			base.SendSuccessResponse(false);
		}

		private BlobType GetBlobTypeFromRequest()
		{
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				return BlobType.ListBlob;
			}
			BlobType blobType = BlobType.None;
			string item = base.RequestHeadersCollection["x-ms-blob-type"];
			if (string.IsNullOrEmpty(item))
			{
				string str = string.Format("Valid {0} values are {1} and {2}.", "x-ms-blob-type", "BlockBlob", "PageBlob");
				throw new RequiredHeaderNotPresentProtocolException("x-ms-blob-type", str);
			}
			blobType = BlobTypeStrings.GetBlobType(item);
			if (blobType == BlobType.None)
			{
				throw new InvalidHeaderProtocolException("x-ms-blob-type", item);
			}
			return blobType;
		}

		private byte[][] GetBlockListFromRequest(Stream bufferedRequestStream, out byte[] calculatedMd5)
		{
			byte[][] numArray;
			using (MD5ReaderStream mD5ReaderStream = new MD5ReaderStream(bufferedRequestStream, base.RequestContentLength, false, base.RequestContext))
			{
				byte[] requestMD5 = base.GetRequestMD5("Content-MD5");
				if (requestMD5 != null)
				{
					mD5ReaderStream.HashToVerifyAgainst = requestMD5;
				}
				byte[][] numArray1 = this.DecodeBlockListFromStream(mD5ReaderStream);
				try
				{
					calculatedMd5 = mD5ReaderStream.Hash;
				}
				catch (InvalidOperationException invalidOperationException)
				{
					throw new InvalidXmlProtocolException("Error parsing Xml content. There is text beyond the root element BlockList", invalidOperationException);
				}
				numArray = numArray1;
			}
			return numArray;
		}

		private void GetBlockListFromRequestV2(Stream bufferedRequestStream, out byte[][] blockIdList, out BlockSource[] blockSourceList, out byte[] calculatedMd5)
		{
			blockIdList = null;
			blockSourceList = null;
			using (MD5ReaderStream mD5ReaderStream = new MD5ReaderStream(bufferedRequestStream, base.RequestContentLength, false, base.RequestContext))
			{
				byte[] requestMD5 = base.GetRequestMD5("Content-MD5");
				if (requestMD5 != null)
				{
					mD5ReaderStream.HashToVerifyAgainst = requestMD5;
				}
				this.DecodeBlockListFromStreamV2(mD5ReaderStream, out blockIdList, out blockSourceList);
				try
				{
					calculatedMd5 = mD5ReaderStream.Hash;
				}
				catch (InvalidOperationException invalidOperationException)
				{
					throw new InvalidXmlProtocolException("Error parsing Xml content. There is text beyond the root element BlockList", invalidOperationException);
				}
			}
		}

		private IEnumerator<IAsyncResult> GetBlockListImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			TimeSpan initialTimeoutForSizeBasedTimeoutOperation = this.GetInitialTimeoutForSizeBasedTimeoutOperation(this.GetBlockListInitialTimeout);
			Duration startingNow = Duration.StartingNow;
			Guid? nullable = this.GetLeaseId(false);
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(null, nullable1, nullable2, nullable3, BlobType.ListBlob, false, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, false, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable4);
			DateTime? nullable5 = null;
			IAsyncResult stream = this.serviceManager.BeginGetBlockList(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, BlockListTypes.Committed, BlobServiceVersion.Original, blobObjectCondition, nullable5, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlockListImpl"));
			yield return stream;
			IBlockCollection committedBlockList = this.serviceManager.EndGetBlockList(stream).CommittedBlockList;
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as IBlobOperationWithResponseContentMeasurementEvent != null);
			this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = committedBlockList.BlockCount;
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				ListBlocksXmlListEncoder listBlocksXmlListEncoder = new ListBlocksXmlListEncoder();
				GetBlockListOperationContext getBlockListOperationContext = new GetBlockListOperationContext("BlockList");
				initialTimeoutForSizeBasedTimeoutOperation = this.AdjustTimeoutForListResult(this.operationContext.OperationMeasurementEvent, (long)HttpRestProcessor.ApproximateByteSizePerBlock, (long)committedBlockList.BlockCount, startingNow, initialTimeoutForSizeBasedTimeoutOperation);
				stream = listBlocksXmlListEncoder.BeginEncodeListToStream(null, committedBlockList, getBlockListOperationContext, stream1, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlockListImpl"));
				yield return stream;
				listBlocksXmlListEncoder.EndEncodeListToStream(stream);
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetBlockListImplV2(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			TimeSpan initialTimeoutForSizeBasedTimeoutOperation = this.GetInitialTimeoutForSizeBasedTimeoutOperation(this.GetBlockListInitialTimeout);
			Duration startingNow = Duration.StartingNow;
			Guid? nullable = this.GetLeaseId(false);
			DateTime? nullable1 = this.GetSnapshot(true, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			string item = base.RequestQueryParameters["blocklisttype"];
			BlockListTypes defaultBlockListType = BlockListTypes.None;
			if (string.IsNullOrEmpty(item))
			{
				defaultBlockListType = HttpRestProcessor.DefaultBlockListType;
			}
			else if (item.Equals("uncommitted", StringComparison.OrdinalIgnoreCase))
			{
				defaultBlockListType = BlockListTypes.Uncommitted;
			}
			else if (!item.Equals("committed", StringComparison.OrdinalIgnoreCase))
			{
				if (!item.Equals("all", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidQueryParameterProtocolException("blocklisttype", item, "This is not a valid block list type.");
				}
				defaultBlockListType = BlockListTypes.All;
			}
			else
			{
				defaultBlockListType = BlockListTypes.Committed;
			}
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			Guid? nullable2 = nullable;
			ComparisonOperator? nullable3 = null;
			long? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(null, nullable2, nullable3, nullable4, BlobType.ListBlob, false, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, false, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable5);
			IAsyncResult stream = this.serviceManager.BeginGetBlockList(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, defaultBlockListType, BlobServiceVersion.Mix, blobObjectCondition, nullable1, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlockListImplV2"));
			yield return stream;
			IBlockLists blockList = this.serviceManager.EndGetBlockList(stream);
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && blockList.BlobLastModificationTime > DateTimeConstants.MinimumBlobLastModificationTime)
			{
				this.AddLastModifiedAndETagToResponse(blockList.BlobLastModificationTime);
				WebHeaderCollection headers = base.Response.Headers;
				headers["x-ms-blob-content-length"] = blockList.BlobSize.ToString();
			}
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as IBlobOperationWithResponseContentMeasurementEvent != null);
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				ListBlocksXmlListEncoder listBlocksXmlListEncoder = new ListBlocksXmlListEncoder();
				List<GetBlockListOperationContext> getBlockListOperationContexts = new List<GetBlockListOperationContext>();
				List<IEnumerable<IBlock>> enumerables = new List<IEnumerable<IBlock>>();
				this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = 0;
				if ((defaultBlockListType & BlockListTypes.Committed) != BlockListTypes.None)
				{
					getBlockListOperationContexts.Add(new GetBlockListOperationContext("CommittedBlocks"));
					enumerables.Add(blockList.CommittedBlockList);
					initialTimeoutForSizeBasedTimeoutOperation = this.AdjustTimeoutForListResult(this.operationContext.OperationMeasurementEvent, (long)HttpRestProcessor.ApproximateByteSizePerBlock, (long)blockList.CommittedBlockList.BlockCount, startingNow, initialTimeoutForSizeBasedTimeoutOperation);
					INephosBaseOperationMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent;
					operationMeasurementEvent.ItemsReturnedCount = operationMeasurementEvent.ItemsReturnedCount + blockList.CommittedBlockList.BlockCount;
				}
				if ((defaultBlockListType & BlockListTypes.Uncommitted) != BlockListTypes.None)
				{
					getBlockListOperationContexts.Add(new GetBlockListOperationContext("UncommittedBlocks"));
					enumerables.Add(blockList.UncommittedBlockList);
					initialTimeoutForSizeBasedTimeoutOperation = this.AdjustTimeoutForListResult(this.operationContext.OperationMeasurementEvent, (long)HttpRestProcessor.ApproximateByteSizePerBlock, (long)blockList.UncommittedBlockList.BlockCount, startingNow, initialTimeoutForSizeBasedTimeoutOperation);
					INephosBaseOperationMeasurementEvent itemsReturnedCount = this.operationContext.OperationMeasurementEvent;
					itemsReturnedCount.ItemsReturnedCount = itemsReturnedCount.ItemsReturnedCount + blockList.UncommittedBlockList.BlockCount;
				}
				stream = listBlocksXmlListEncoder.BeginEncodeListsToStream(null, enumerables, getBlockListOperationContexts, "BlockList", stream1, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetBlockListImplV2"));
				yield return stream;
				listBlocksXmlListEncoder.EndEncodeListToStream(stream);
			}
			base.SendSuccessResponse(false);
		}

		private PutBlobProperties GetCommitBlobPropertiesFromRequest()
		{
			long? requestCrc64;
			bool? requestPutBlobComputeMD5;
			PutBlobProperties putBlobProperty = new PutBlobProperties();
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				putBlobProperty.ContentEncoding = base.RequestHeadersCollection["Content-Encoding"];
				putBlobProperty.ContentLanguage = base.RequestHeadersCollection["Content-Language"];
				putBlobProperty.ContentType = base.RequestContentType;
				putBlobProperty.ContentMD5 = base.GetRequestMD5("x-ms-prop-blob-content-md5");
			}
			else
			{
				putBlobProperty.CacheControl = base.RequestHeadersCollection["x-ms-blob-cache-control"];
				putBlobProperty.ContentEncoding = base.RequestHeadersCollection["x-ms-blob-content-encoding"];
				putBlobProperty.ContentLanguage = base.RequestHeadersCollection["x-ms-blob-content-language"];
				putBlobProperty.ContentType = base.RequestHeadersCollection["x-ms-blob-content-type"];
				putBlobProperty.ContentMD5 = base.GetRequestMD5("x-ms-blob-content-md5");
				PutBlobProperties putBlobProperty1 = putBlobProperty;
				if (base.SupportCrc64)
				{
					requestCrc64 = base.GetRequestCrc64("x-ms-blob-content-crc64");
				}
				else
				{
					requestCrc64 = null;
				}
				putBlobProperty1.ContentCrc64 = requestCrc64;
				PutBlobProperties putBlobProperty2 = putBlobProperty;
				if (base.SupportCrc64)
				{
					requestPutBlobComputeMD5 = base.GetRequestPutBlobComputeMD5("x-ms-put-blob-compute-md5");
				}
				else
				{
					requestPutBlobComputeMD5 = null;
				}
				putBlobProperty2.PutBlobComputeMD5 = requestPutBlobComputeMD5;
				if (base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					putBlobProperty.ContentDisposition = base.RequestHeadersCollection["x-ms-blob-content-disposition"];
				}
			}
			this.GetCommonPutBlobPropertiesFromRequest(putBlobProperty);
			return putBlobProperty;
		}

		private void GetCommonPutBlobPropertiesFromRequest(PutBlobProperties blobProperties)
		{
			blobProperties.CreationTime = base.ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-prop-creation-time");
			blobProperties.LastModifiedTime = base.ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "Last-Modified");
			base.AddMetadataFromRequest(blobProperties.BlobMetadata);
			IBlobContentMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as IBlobContentMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			operationMeasurementEvent.ContentEncoding = blobProperties.ContentEncoding;
			operationMeasurementEvent.ContentLanguage = blobProperties.ContentLanguage;
			operationMeasurementEvent.CacheControl = blobProperties.CacheControl;
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && string.IsNullOrEmpty(blobProperties.ContentType))
			{
				blobProperties.ContentType = "application/octet-stream";
			}
		}

		private IEnumerator<IAsyncResult> GetContainerAclImpl(AsyncIteratorContext<NoResults> async)
		{
			SASPermission? nullable;
			bool flag;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable1 = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable1 = this.GetLeaseId(false);
			}
			IAsyncResult stream = this.serviceManager.BeginGetContainerAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nullable1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetContainerAclImpl"));
			yield return stream;
			ContainerAclSettings containerAclSetting = null;
			DateTime dateTime = this.serviceManager.EndGetContainerAcl(stream, out containerAclSetting);
			if (!base.RequestContext.IsRequestVersionAtLeastApril15)
			{
				foreach (SASIdentifier sASIdentifier in containerAclSetting.SASIdentifiers)
				{
					if (!sASIdentifier.AccessPolicy.SignedPermission.HasValue)
					{
						continue;
					}
					SASPermission? signedPermission = sASIdentifier.AccessPolicy.SignedPermission;
					if (signedPermission.HasValue)
					{
						nullable = new SASPermission?(signedPermission.GetValueOrDefault() & (SASPermission.Add | SASPermission.Update | SASPermission.Process | SASPermission.Create));
					}
					else
					{
						nullable = null;
					}
					SASPermission? nullable2 = nullable;
					flag = (nullable2.GetValueOrDefault() != SASPermission.None ? true : !nullable2.HasValue);
					if (!flag)
					{
						continue;
					}
					throw new UnsupportedPermissionInStoredAccessPolicyException();
				}
			}
			base.SetContainerAclOnResponse(containerAclSetting);
			this.AddLastModifiedAndETagToResponse(dateTime);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			if (!base.RequestContext.IsRequestVersionAtLeastJuly09 || base.Method == RestMethod.HEAD)
			{
				T timeSpan = this.operationContext;
				timeSpan.OperationClientWriteNetworkLatency = new TimeSpan(this.operationContext.OperationClientWriteNetworkLatency.Ticks);
			}
			else
			{
				ListContainersAclXmlListEncoder listContainersAclXmlListEncoder = new ListContainersAclXmlListEncoder(base.RequestContext.IsRequestVersionAtLeastApril17);
				using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
				{
					base.Response.SendChunked = true;
					base.Response.ContentType = "application/xml";
					stream = listContainersAclXmlListEncoder.BeginEncodeListToStream(null, containerAclSetting.SASIdentifiers, containerAclSetting, stream1, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetContainerAclImpl"));
					yield return stream;
					listContainersAclXmlListEncoder.EndEncodeListToStream(stream);
				}
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetContainerMetadataImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId(false);
			}
			DateTime? nullable1 = null;
			DateTime? nullable2 = null;
			IAsyncResult asyncResult = this.serviceManager.BeginGetContainerMetadata(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nullable2, nullable1, nullable, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetContainerMetadataImpl"));
			yield return asyncResult;
			IContainerProperties containerProperty = this.serviceManager.EndGetContainerMetadata(asyncResult);
			NameValueCollection containerMetadata = containerProperty.ContainerMetadata;
			DateTime lastModifiedTime = containerProperty.LastModifiedTime;
			this.ThrowBlobErrorForDoubleLoookupIfNecessary(containerProperty.ContainerAcl);
			if (containerMetadata != null)
			{
				base.SetMetadataOnResponse(containerMetadata);
			}
			this.AddLastModifiedAndETagToResponse(lastModifiedTime);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> GetContainerPropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId(false);
			}
			DateTime? nullable1 = null;
			DateTime? nullable2 = null;
			IAsyncResult asyncResult = this.serviceManager.BeginGetContainerProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nullable2, nullable1, nullable, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.GetContainerPropertiesImpl"));
			yield return asyncResult;
			IContainerProperties containerProperty = this.serviceManager.EndGetContainerProperties(asyncResult);
			this.ThrowBlobErrorForDoubleLoookupIfNecessary(containerProperty.ContainerAcl);
			this.SetContainerPropertiesOnResponse(containerProperty);
			if (base.RequestContext.IsRequestVersionAtLeastMay16)
			{
				base.SetContainerAclOnResponse(containerProperty.ContainerAcl);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		public CopyAction GetCopyAction(bool isCopyActionRequired)
		{
			CopyAction copyAction = CopyAction.None;
			string item = base.RequestHeadersCollection["x-ms-copy-action"];
			if (!string.IsNullOrEmpty(item))
			{
				if (!item.Equals("abort", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidHeaderProtocolException("x-ms-copy-action", item);
				}
				copyAction = CopyAction.Abort;
			}
			else if (isCopyActionRequired)
			{
				throw new RequiredHeaderNotPresentProtocolException("x-ms-copy-action");
			}
			return copyAction;
		}

		protected CopyBlobProperties GetCopyBlobPropertiesFromRequest()
		{
			CopyBlobProperties copyBlobProperty = new CopyBlobProperties();
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			if (nameValueCollection.Count <= 0)
			{
				NephosAssertionException.Assert(copyBlobProperty.BlobMetadata == null);
			}
			else
			{
				copyBlobProperty.BlobMetadata = nameValueCollection;
			}
			return copyBlobProperty;
		}

		public Guid? GetCopyId(bool isRequired)
		{
			Guid? nullable = null;
			string item = base.RequestQueryParameters["copyid"];
			if (!string.IsNullOrEmpty(item))
			{
				try
				{
					nullable = new Guid?(new Guid(item));
				}
				catch (FormatException formatException)
				{
					throw new InvalidQueryParameterProtocolException("copyid", item, "copyid needs to be valid Guid.");
				}
				catch (OverflowException overflowException)
				{
					throw new InvalidQueryParameterProtocolException("copyid", item, "copyid needs to be valid Guid.");
				}
			}
			if (isRequired && !nullable.HasValue)
			{
				throw new NullOrEmptyQueryParameterProtocolException("copyid");
			}
			return nullable;
		}

		public FECopyType GetCopyType(string copyTypeQueryParam)
		{
			FECopyType fECopyType = FECopyType.Default;
			if (!string.IsNullOrEmpty(copyTypeQueryParam))
			{
				if (!copyTypeQueryParam.Equals("incremental", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidQueryParameterProtocolException("copytype", copyTypeQueryParam, null);
				}
				fECopyType = FECopyType.Incremental;
			}
			return fECopyType;
		}

		public TimeSpan? GetDuration(string durationHeaderName, bool isDurationRequired)
		{
			long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, durationHeaderName, null);
			if (isDurationRequired && !nullable.HasValue)
			{
				throw new RequiredHeaderNotPresentProtocolException(durationHeaderName);
			}
			if (!nullable.HasValue)
			{
				return null;
			}
			return new TimeSpan?(TimeSpan.FromSeconds((double)nullable.Value));
		}

		private NephosErrorDetails GetErrorDetailsForBlobFeSpecificException(Exception e)
		{
			NephosStatusEntry nephosStatusEntry;
			if (e is BlobNotFoundException)
			{
				BlobNotFoundException blobNotFoundException = e as BlobNotFoundException;
				if (this.operationContext.IsRequestAnonymous)
				{
					return new NephosErrorDetails(CommonStatusEntries.BlobNotFound, NephosRESTEventStatus.AuthorizationFailure, blobNotFoundException);
				}
				return new NephosErrorDetails(CommonStatusEntries.BlobNotFound, NephosRESTEventStatus.ExpectedFailure, blobNotFoundException);
			}
			if (e is BlobWriteProtectedException)
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] logString = new object[] { e.GetLogString() };
				error.Log("Failed to write a write protected blob: {0}", logString);
				return new NephosErrorDetails(BlobStatusEntries.BlobWriteProtected, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is CopySourceCannotBeIncrementalCopyBlobException)
			{
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray = new object[] { e.GetLogString() };
				stringDataEventStream.Log("Source blob of a copy operation cannot be an incremental copy blob: {0}", objArray);
				return new NephosErrorDetails(BlobStatusEntries.CopySourceCannotBeIncrementalCopyBlob, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is OperationNotAllowedOnIncrementalCopyBlobException)
			{
				IStringDataEventStream error1 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] logString1 = new object[] { e.GetLogString() };
				error1.Log("Failed to read or write to incremental copy blob: {0}", logString1);
				return new NephosErrorDetails(BlobStatusEntries.OperationNotAllowedOnIncrementalCopyBlob, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is IncrementalCopyOfEarlierSnapshotNotAllowedException)
			{
				IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray1 = new object[] { e.GetLogString() };
				stringDataEventStream1.Log("The specified snapshot is earlier than the last snapshot copied into the incremental copy blob: {0}", objArray1);
				return new NephosErrorDetails(BlobStatusEntries.IncrementalCopyOfEarlierSnapshotNotAllowed, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is IncrementalCopyBlobMismatchException)
			{
				IStringDataEventStream error2 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] logString2 = new object[] { e.GetLogString() };
				error2.Log("The specified source blob is different than the copy source of the existing incremental copy blob: {0}", logString2);
				return new NephosErrorDetails(BlobStatusEntries.IncrementalCopyBlobMismatch, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is IncrementalCopyBlobPreviousSnapshotDoesNotExistException)
			{
				IStringDataEventStream stringDataEventStream2 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray2 = new object[] { e.GetLogString() };
				stringDataEventStream2.Log("The snapshot or the previous snapshot of the source blob does not exist: {0}", objArray2);
				return new NephosErrorDetails(BlobStatusEntries.IncrementalCopyBlobPreviousSnapshotDoesNotExist, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobAlreadyExistsException)
			{
				return new NephosErrorDetails(BlobStatusEntries.BlobAlreadyExists, NephosRESTEventStatus.ExpectedFailure, e as BlobAlreadyExistsException);
			}
			if (e is InvalidBlockException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidBlobOrBlock, NephosRESTEventStatus.ExpectedFailure, e as InvalidBlockException);
			}
			if (e is TimeoutException)
			{
				return base.GetErrorDetailsForTimeoutException(e);
			}
			if (e is InvalidBlockListException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidBlockList, NephosRESTEventStatus.ExpectedFailure, e as InvalidBlockListException);
			}
			if (e is InvalidBlobRegionException)
			{
				InvalidBlobRegionException invalidBlobRegionException = e as InvalidBlobRegionException;
				NameValueCollection nameValueCollection = null;
				if (invalidBlobRegionException.BlobContentLength.HasValue)
				{
					nameValueCollection = new NameValueCollection(1);
					long? blobContentLength = invalidBlobRegionException.BlobContentLength;
					nameValueCollection.Add("Content-Range", string.Format("bytes */{0}", blobContentLength.Value));
				}
				return new NephosErrorDetails(CommonStatusEntries.InvalidRange, NephosRESTEventStatus.ExpectedFailure, invalidBlobRegionException, nameValueCollection, null);
			}
			if (e is PageRangeInvalidException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidPageRange, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is SequenceNumberIncrementTooLargeException)
			{
				return new NephosErrorDetails(BlobStatusEntries.SequenceNumberIncrementTooLarge, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobContentTooLargeException)
			{
				BlobContentTooLargeException blobContentTooLargeException = e as BlobContentTooLargeException;
				return base.GetErrorDetailsForException(new RequestEntityTooLargeException(blobContentTooLargeException.MaxAllowedSize, blobContentTooLargeException));
			}
			if (e is BlockListTooLongException)
			{
				if (base.RequestContext.IsRequestVersionAtLeastMay16)
				{
					return new NephosErrorDetails(BlobStatusEntries.BlockListTooLong, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return base.GetErrorDetailsForException(new RequestEntityTooLargeException(new long?((long)50000), e));
			}
			if (e is UncommittedBlockCountExceedsLimitException)
			{
				if (base.RequestContext.IsRequestVersionAtLeastMay16)
				{
					return new NephosErrorDetails(BlobStatusEntries.UncommittedBlockCountExceedsLimit, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return base.GetErrorDetailsForException(new RequestEntityTooLargeException(new long?((long)100000), e));
			}
			if (e is BlobModifiedWhileReadingException)
			{
				return new NephosErrorDetails(BlobStatusEntries.BlobModifiedWhileReading, NephosRESTEventStatus.ExpectedFailure, e as BlobModifiedWhileReadingException);
			}
			if (e is CannotCopyAcrossAccountsException)
			{
				NameValueCollection nameValueCollection1 = new NameValueCollection();
				CannotCopyAcrossAccountsException cannotCopyAcrossAccountsException = (CannotCopyAcrossAccountsException)e;
				nameValueCollection1.Add("SourceAccountName", cannotCopyAcrossAccountsException.SourceAccountName);
				nameValueCollection1.Add("DestinationAccountName", cannotCopyAcrossAccountsException.DestinationAccountName);
				return new NephosErrorDetails(BlobStatusEntries.CopyAcrossAccountsNotSupported, NephosRESTEventStatus.ExpectedFailure, e, null, nameValueCollection1);
			}
			if (e is CannotVerifyCopySourceException)
			{
				CannotVerifyCopySourceException cannotVerifyCopySourceException = (CannotVerifyCopySourceException)e;
				nephosStatusEntry = (cannotVerifyCopySourceException.StatusCode < HttpStatusCode.InternalServerError ? new NephosStatusEntry("CannotVerifyCopySource", cannotVerifyCopySourceException.StatusCode, cannotVerifyCopySourceException.StatusDescription) : BlobStatusEntries.AuthorizingCopySourceTimedOut);
				return new NephosErrorDetails(nephosStatusEntry, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is PendingCopyException)
			{
				return new NephosErrorDetails(BlobStatusEntries.PendingCopyOperation, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is CopyIdMismatchException)
			{
				return new NephosErrorDetails(BlobStatusEntries.CopyIdMismatch, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is IncrementalCopySourceMustBeSnapshotException)
			{
				return new NephosErrorDetails(BlobStatusEntries.IncrementalCopySourceMustBeSnapshot, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidSourceBlobTypeException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidSourceBlobType, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidSourceBlobUrlException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidSourceBlobUrl, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is NoPendingCopyException)
			{
				return new NephosErrorDetails(BlobStatusEntries.NoPendingCopyOperation, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is LeaseBrokenException)
			{
				if (this.operationContext.IsAcquireLeaseOperation)
				{
					return new NephosErrorDetails(BlobStatusEntries.LeaseIsBreakingAndCannotBeAcquired, NephosRESTEventStatus.ExpectedFailure, e);
				}
				if (this.operationContext.IsRenewLeaseOperation)
				{
					return new NephosErrorDetails(BlobStatusEntries.LeaseIsBrokenAndCannotBeRenewed, NephosRESTEventStatus.ExpectedFailure, e);
				}
				if (this.operationContext.IsBreakLeaseOperation)
				{
					return new NephosErrorDetails(BlobStatusEntries.LeaseAlreadyBroken, NephosRESTEventStatus.ExpectedFailure, e);
				}
				if (!this.operationContext.IsChangeLeaseOperation)
				{
					return null;
				}
				return new NephosErrorDetails(BlobStatusEntries.LeaseIsBreakingAndCannotBeChanged, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is LeaseHeldException)
			{
				if (this.operationContext.IsAcquireLeaseOperation)
				{
					return new NephosErrorDetails(BlobStatusEntries.LeaseAlreadyPresent, NephosRESTEventStatus.ExpectedFailure, e);
				}
				if (this.operationContext.IsChangeLeaseOperation)
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithBlobLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithContainerLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				else if (string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-lease-id"]))
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMissingWithBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMissingWithContainerOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				else
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithContainerOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				return null;
			}
			if (e is LeaseLostException)
			{
				if (this.operationContext.IsRenewLeaseOperation || this.operationContext.IsReleaseLeaseOperation)
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithBlobLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseIdMismatchWithContainerLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				else if (this.operationContext.IsChangeLeaseOperation || this.operationContext.IsBreakLeaseOperation)
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithBlobLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithContainerLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				else
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseLostWithBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseLostWithContainerOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				return null;
			}
			if (e is LeaseNotPresentException)
			{
				if (this.operationContext.IsBreakLeaseOperation || this.operationContext.IsChangeLeaseOperation)
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithBlobLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithContainerLeaseOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				else
				{
					if (this.operationContext.ResourceIsBlobOrFilePath)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
					if (this.operationContext.ResourceIsContainer)
					{
						return new NephosErrorDetails(BlobStatusEntries.LeaseNotPresentWithContainerOperation, NephosRESTEventStatus.ExpectedFailure, e);
					}
				}
				return null;
			}
			if (e is LeaseDurationNotInfiniteException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InfiniteLeaseDurationRequired, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is SnapshotsPresentException)
			{
				return new NephosErrorDetails(BlobStatusEntries.SnapshotsPresent, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidBlobTypeException)
			{
				if (!(e as InvalidBlobTypeException).IsPageBlobInvalidVersion)
				{
					return new NephosErrorDetails(BlobStatusEntries.InvalidBlobType, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return new NephosErrorDetails(BlobStatusEntries.InvalidVersionForPageBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidVersionForCopyAppendBlobOperationException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidVersionForAppendBlobOperation, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is InvalidVersionForBlobTypeInBlobListException)
			{
				return new NephosErrorDetails(BlobStatusEntries.InvalidVersionForBlobTypeInBlobList, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is AppendPositionConditionNotMetException)
			{
				return new NephosErrorDetails(BlobStatusEntries.AppendPositionConditionNotMet, NephosRESTEventStatus.ConditionNotMetFailure, e);
			}
			if (e is MaxBlobSizeConditionNotMetException)
			{
				return new NephosErrorDetails(BlobStatusEntries.MaxBlobSizeConditionNotMet, NephosRESTEventStatus.ConditionNotMetFailure, e);
			}
			if (e is BlockCountExceedsLimitException)
			{
				return new NephosErrorDetails((base.RequestContext.IsRequestVersionAtLeastMay16 ? BlobStatusEntries.May16BlockCountExceedsLimit : BlobStatusEntries.BlockCountExceedsLimit), NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobPreviousSnapshotNotFoundException)
			{
				return new NephosErrorDetails(BlobStatusEntries.PreviousSnapshotNotFound, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobPreviousSnapshotGenerationIdNotFoundException || e is BlobGenerationIdNotFoundException)
			{
				return new NephosErrorDetails(BlobStatusEntries.DifferentialGetPageRangesNotSupportedOnPreviousSnapshot, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobGenerationIdMismatchException)
			{
				if (base.RequestContext.IsRequestVersionAtLeastMay16)
				{
					return new NephosErrorDetails(BlobStatusEntries.BlobOverwritten, NephosRESTEventStatus.ExpectedFailure, e);
				}
				return new NephosErrorDetails(BlobStatusEntries.BlobGenerationMismatch, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (e is BlobPreviousSnapshotTooNewException)
			{
				return new NephosErrorDetails(BlobStatusEntries.PreviousSnapshotCannotBeNewer, NephosRESTEventStatus.ExpectedFailure, e);
			}
			if (!(e is NephosUnauthorizedAccessException))
			{
				return null;
			}
			if ((e as NephosUnauthorizedAccessException).FailureReason != AuthorizationFailureReason.UnauthorizedBlobOverwrite || !base.RequestContext.IsRequestVersionAtLeastFebruary16)
			{
				return null;
			}
			Logger<IRestProtocolHeadLogger>.Instance.AuthorizationFailure.Log("Unauthorized blob overwrite");
			MeasurementEventStatus authorizationFailure = NephosRESTEventStatus.AuthorizationFailure;
			AccountSasAccessIdentifier callerIdentity = null;
			SignedAccessAccountIdentifier signedAccessAccountIdentifier = null;
			if (this.operationContext != null)
			{
				callerIdentity = this.operationContext.CallerIdentity as AccountSasAccessIdentifier;
				signedAccessAccountIdentifier = this.operationContext.CallerIdentity as SignedAccessAccountIdentifier;
			}
			if (callerIdentity != null || signedAccessAccountIdentifier != null)
			{
				authorizationFailure = NephosRESTEventStatus.PermissionFailureSAS;
			}
			return new NephosErrorDetails(BlobStatusEntries.UnauthorizedBlobOverwrite, authorizationFailure, e, null);
		}

		public override NephosErrorDetails GetErrorDetailsForException(Exception e)
		{
			NephosErrorDetails errorDetailsForBlobFeSpecificException = null;
			if (this.operationContext != null && (this.operationContext.ResourceIsBlobOrFilePath || this.operationContext.ResourceIsContainer))
			{
				errorDetailsForBlobFeSpecificException = this.GetErrorDetailsForBlobFeSpecificException(e);
				if (errorDetailsForBlobFeSpecificException != null)
				{
					return errorDetailsForBlobFeSpecificException;
				}
			}
			return base.GetErrorDetailsForException(e);
		}

		protected TimeSpan GetInitialTimeoutForSizeBasedTimeoutOperation(TimeSpan minimumSizeBasedTimeout)
		{
			TimeSpan timeSpan = this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout);
			TimeSpan timeSpan1 = (minimumSizeBasedTimeout > timeSpan ? timeSpan : minimumSizeBasedTimeout);
			this.operationContext.MaxAllowedTimeout = minimumSizeBasedTimeout;
			return timeSpan1;
		}

		protected bool GetIsCalculatingRangeCrc64FromRequest(bool isRangeSpecified, bool isEndOffsetSpecified, long length)
		{
			bool? nullable = base.ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-range-get-content-crc64");
			return this.IsSpecifiedRangeValidForCalculatingCrc64OrMd5(nullable, "x-ms-range-get-content-crc64", isRangeSpecified, isEndOffsetSpecified, length, (long)4194304);
		}

		protected bool GetIsCalculatingRangeMD5FromRequest(bool isRangeSpecified, bool isEndOffsetSpecified, long length)
		{
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				return false;
			}
			bool? nullable = base.ParseOptionalBoolInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-range-get-content-md5");
			return this.IsSpecifiedRangeValidForCalculatingCrc64OrMd5(nullable, "x-ms-range-get-content-md5", isRangeSpecified, isEndOffsetSpecified, length, (long)4194304);
		}

		public TimeSpan? GetLeaseBreakPeriod()
		{
			if (!base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				return null;
			}
			TimeSpan? duration = this.GetDuration("x-ms-lease-break-period", false);
			if (duration.HasValue && (duration.Value.TotalSeconds < 0 || duration.Value > HttpRestProcessor.LeaseDurationMaxSeconds))
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] item = new object[] { base.RequestHeadersCollection["x-ms-lease-break-period"], "x-ms-lease-break-period", HttpRestProcessor.LeaseDurationMaxSeconds.TotalSeconds };
				error.Log("Received value {0} for header {1} is invalid. Valid range is 0 to {2} seconds.", item);
				throw new InvalidHeaderProtocolException("x-ms-lease-break-period", base.RequestHeadersCollection["x-ms-lease-break-period"]);
			}
			return duration;
		}

		public TimeSpan GetLeaseDuration()
		{
			if (!base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				return HttpRestProcessor.DefaultLeaseDuration;
			}
			TimeSpan? duration = this.GetDuration("x-ms-lease-duration", true);
			NephosAssertionException.Assert(duration.HasValue, "Expected lease duration to have a value or for a protocol exception to be thrown.");
			if (duration.Value != RealServiceManager.LeaseDurationInfinite && (duration.Value < HttpRestProcessor.LeaseDurationMinSeconds || duration.Value > HttpRestProcessor.LeaseDurationMaxSeconds))
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] item = new object[] { base.RequestHeadersCollection["x-ms-lease-duration"], "x-ms-lease-duration", HttpRestProcessor.LeaseDurationMinSeconds.TotalSeconds, HttpRestProcessor.LeaseDurationMaxSeconds.TotalSeconds, RealServiceManager.LeaseDurationInfinite.TotalSeconds };
				error.Log("Received value {0} for header {1} is invalid. Valid range is {2} to {3} seconds, or {4} for infinite.", item);
				throw new InvalidHeaderProtocolException("x-ms-lease-duration", base.RequestHeadersCollection["x-ms-lease-duration"]);
			}
			return duration.Value;
		}

		public Guid? GetLeaseId(bool isLeaseIdRequired)
		{
			return this.GetLeaseId("x-ms-lease-id", isLeaseIdRequired);
		}

		public Guid? GetLeaseId(string leaseIdHeaderName, bool isLeaseIdRequired)
		{
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				return null;
			}
			Guid? nullable = null;
			string item = base.RequestHeadersCollection[leaseIdHeaderName];
			try
			{
				if (!string.IsNullOrEmpty(item))
				{
					nullable = new Guid?(new Guid(item));
				}
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException(leaseIdHeaderName, item);
			}
			catch (OverflowException overflowException)
			{
				throw new InvalidHeaderProtocolException(leaseIdHeaderName, item);
			}
			if (isLeaseIdRequired && !nullable.HasValue)
			{
				throw new RequiredHeaderNotPresentProtocolException(leaseIdHeaderName);
			}
			if (nullable.HasValue && !string.IsNullOrEmpty(base.RequestQueryParameters["snapshot"]))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Leases are not allowed with snapshots.");
				throw new InvalidQueryParameterProtocolException("snapshot", base.RequestQueryParameters["snapshot"], "Leases are not allowed with snapshots.");
			}
			return nullable;
		}

		private long? GetMaxSizeBlobConditionFromRequest()
		{
			return base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-blob-condition-maxsize", null);
		}

		private IEnumerator<IAsyncResult> GetPageRangeListImpl(AsyncIteratorContext<NoResults> asyncContext)
		{
			long nextPageStart;
			long num;
			bool flag;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			TimeSpan initialTimeoutForSizeBasedTimeoutOperation = this.GetInitialTimeoutForSizeBasedTimeoutOperation(this.GetPageRangeListInitialTimeout);
			Duration startingNow = Duration.StartingNow;
			Guid? nullable = this.GetLeaseId(false);
			DateTime? nullable1 = this.GetSnapshot(true, false, BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "snapshot");
			this.Range = HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection);
			this.GetRangeFromRequestAndAdjustEndOffset(base.RequestHeadersCollection, base.RequestContext.IsRequestVersionAtLeastAugust13, out nextPageStart, out num, out flag);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.ReadOperation, base.RequestRestVersion);
			bool flag1 = true;
			DateTime? nullable2 = null;
			bool flag2 = true;
			if (base.RequestContext.IsRequestVersionAtLeastJuly15)
			{
				nullable2 = base.ParseOptionalDateTimeInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "prevsnapshot", true);
				if (nullable2.HasValue)
				{
					flag2 = false;
				}
			}
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			base.StatusCode = HttpStatusCode.OK;
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as IBlobOperationWithResponseContentMeasurementEvent != null);
			using (Stream stream = base.GenerateMeasuredResponseStream(false))
			{
				ListPageRangesXmlListEncoder listPageRangesXmlListEncoder = new ListPageRangesXmlListEncoder(true);
				IPageRangeCollection pageRangeCollection = null;
				GetPageRangeListOperationContext getPageRangeListOperationContext = new GetPageRangeListOperationContext(false, num);
				int num1 = 0;
				bool flag3 = false;
				HttpRestProcessor httpRestProcessor = this;
				ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
				Guid? nullable3 = nullable;
				ComparisonOperator? nullable4 = null;
				long? nullable5 = null;
				DateTime? nullable6 = null;
				Guid? nullable7 = null;
				BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable3, nullable4, nullable5, BlobType.IndexBlob, flag3, false, false, false, nullable6, null, nullable7);
				using (BlobPropertyNames blobPropertyName = BlobPropertyNames.None)
				{
					while (true)
					{
						IAsyncResult asyncResult = this.serviceManager.BeginGetPageRangeList(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nextPageStart, num - nextPageStart + (long)1, blobPropertyName, blobObjectCondition, nullable1, HttpRestProcessor.SingleGetPageRangeListMaxPageRanges, nullable2, flag1, flag2, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), base.RequestContext, asyncContext.GetResumeCallback(), asyncContext.GetResumeState("HttpRestProcessor.GetPageRangeListImpl"));
						yield return asyncResult;
						IGetPageRangeListResult getPageRangeListResult = this.serviceManager.EndGetPageRangeList(asyncResult);
						pageRangeCollection = getPageRangeListResult.PageRangeCollection;
						initialTimeoutForSizeBasedTimeoutOperation = this.AdjustTimeoutForListResult(this.operationContext.OperationMeasurementEvent, (long)HttpRestProcessor.ApproximateByteSizePerPageRange, (long)pageRangeCollection.PageRangeCount, startingNow, initialTimeoutForSizeBasedTimeoutOperation);
						if (num1 == 0)
						{
							blobObjectCondition = new BlobObjectCondition()
							{
								IfModifiedSinceTime = null,
								IfNotModifiedSinceTime = new DateTime?(getPageRangeListResult.BlobLastModificationTime),
								LeaseId = nullable
							};
							this.AddLastModifiedAndETagToResponse(getPageRangeListResult.BlobLastModificationTime);
							WebHeaderCollection headers = base.Response.Headers;
							headers["x-ms-blob-content-length"] = getPageRangeListResult.ContentLength.ToString();
							base.AddServiceResponseHeadersBeforeSendingResponse();
						}
						this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = pageRangeCollection.PageRangeCount;
						asyncResult = listPageRangesXmlListEncoder.BeginEncodeListToStream(null, pageRangeCollection, getPageRangeListOperationContext, stream, startingNow.Remaining(initialTimeoutForSizeBasedTimeoutOperation), asyncContext.GetResumeCallback(), asyncContext.GetResumeState("ListPageRangesXmlListEncoder.EncodeListToStream"));
						yield return asyncResult;
						listPageRangesXmlListEncoder.EndEncodeListToStream(asyncResult);
						if (pageRangeCollection.HasMoreRows && pageRangeCollection.NextPageStart < num)
						{
							nextPageStart = pageRangeCollection.NextPageStart;
							getPageRangeListOperationContext.IsContinuing = true;
							num1++;
						}
						if (!pageRangeCollection.HasMoreRows || pageRangeCollection.NextPageStart >= num)
						{
							break;
						}
					}
				}
				listPageRangesXmlListEncoder.Complete();
			}
			base.SendSuccessResponse(false);
		}

		public Guid? GetProposedLeaseId(bool isLeaseIdRequired)
		{
			if (!base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				return null;
			}
			return this.GetLeaseId("x-ms-proposed-lease-id", isLeaseIdRequired);
		}

		private PutBlobProperties GetPutBlobPropertiesFromRequest()
		{
			bool? requestPutBlobComputeMD5;
			PutBlobProperties putBlobProperty = new PutBlobProperties()
			{
				BlobType = this.GetBlobTypeFromRequest()
			};
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				putBlobProperty.ContentMD5 = base.GetRequestMD5("Content-MD5");
			}
			else
			{
				putBlobProperty.CacheControl = base.RequestHeadersCollection["x-ms-blob-cache-control"];
				putBlobProperty.ContentEncoding = base.RequestHeadersCollection["x-ms-blob-content-encoding"];
				putBlobProperty.ContentLanguage = base.RequestHeadersCollection["x-ms-blob-content-language"];
				putBlobProperty.ContentType = base.RequestHeadersCollection["x-ms-blob-content-type"];
				if (base.RequestContext.IsRequestVersionAtLeastAugust13)
				{
					putBlobProperty.ContentDisposition = base.RequestHeadersCollection["x-ms-blob-content-disposition"];
				}
				putBlobProperty.MaxBlobSize = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-blob-content-length", null);
				if (base.SupportCrc64)
				{
					putBlobProperty.ContentCrc64 = base.GetRequestCrc64("x-ms-blob-content-crc64");
					if (!putBlobProperty.ContentCrc64.HasValue)
					{
						if (putBlobProperty.BlobType != BlobType.ListBlob && base.RequestHeadersCollection["x-ms-content-crc64"] != null)
						{
							throw new InvalidHeaderProtocolException("x-ms-content-crc64", base.RequestHeadersCollection["x-ms-content-crc64"]);
						}
						putBlobProperty.ContentCrc64 = base.GetRequestCrc64("x-ms-content-crc64");
					}
				}
				PutBlobProperties putBlobProperty1 = putBlobProperty;
				if (base.SupportCrc64)
				{
					requestPutBlobComputeMD5 = base.GetRequestPutBlobComputeMD5("x-ms-put-blob-compute-md5");
				}
				else
				{
					requestPutBlobComputeMD5 = null;
				}
				putBlobProperty1.PutBlobComputeMD5 = requestPutBlobComputeMD5;
				if (putBlobProperty.BlobType == BlobType.ListBlob)
				{
					putBlobProperty.ContentMD5 = base.GetRequestMD5("x-ms-blob-content-md5");
					if (putBlobProperty.ContentMD5 == null)
					{
						putBlobProperty.ContentMD5 = base.GetRequestMD5("Content-MD5");
					}
				}
				else if (putBlobProperty.BlobType == BlobType.IndexBlob || putBlobProperty.BlobType == BlobType.AppendBlob)
				{
					putBlobProperty.ContentMD5 = base.GetRequestMD5("x-ms-blob-content-md5");
				}
				bool? putBlobComputeMD5 = putBlobProperty.PutBlobComputeMD5;
				if ((!putBlobComputeMD5.GetValueOrDefault() ? false : putBlobComputeMD5.HasValue) && (putBlobProperty.ContentCrc64.HasValue || putBlobProperty.ContentMD5 != null))
				{
					throw new ComputeMd5HeaderUsedWithOtherCrc64Md5HeaderException();
				}
				long? sequenceNumberFromRequest = this.GetSequenceNumberFromRequest();
				if (sequenceNumberFromRequest.HasValue)
				{
					if (putBlobProperty.BlobType != BlobType.IndexBlob)
					{
						throw new InvalidHeaderProtocolException("x-ms-blob-sequence-number", base.RequestHeadersCollection["x-ms-blob-sequence-number"]);
					}
					putBlobProperty.SequenceNumberUpdate = new SequenceNumberUpdate(SequenceNumberUpdateType.Update, sequenceNumberFromRequest.Value);
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && string.IsNullOrEmpty(putBlobProperty.CacheControl))
			{
				putBlobProperty.CacheControl = base.RequestHeadersCollection["Cache-Control"];
			}
			if (string.IsNullOrEmpty(putBlobProperty.ContentEncoding))
			{
				putBlobProperty.ContentEncoding = base.RequestHeadersCollection["Content-Encoding"];
			}
			if (string.IsNullOrEmpty(putBlobProperty.ContentLanguage))
			{
				putBlobProperty.ContentLanguage = base.RequestHeadersCollection["Content-Language"];
			}
			if (string.IsNullOrEmpty(putBlobProperty.ContentType))
			{
				putBlobProperty.ContentType = base.RequestContentType;
			}
			this.GetCommonPutBlobPropertiesFromRequest(putBlobProperty);
			return putBlobProperty;
		}

		public static bool GetRangeFromRequest(NameValueCollection headers, out long startOffset, out long endOffset)
		{
			return HttpRestProcessor.GetRangeFromRequest(HttpRequestAccessorCommon.GetRangeHeaderValue(headers), out startOffset, out endOffset);
		}

		public static bool GetRangeFromRequest(string rangeHeaderValue, out long startOffset, out long endOffset)
		{
			long num = (long)0;
			long num1 = num;
			endOffset = num;
			startOffset = num1;
			return HttpRestProcessor.GetRangeFromRequest(rangeHeaderValue, false, out startOffset, out endOffset);
		}

		public static bool GetRangeFromRequest(NameValueCollection headers, bool allowUnspecifiedEndOffset, out long startOffset, out long endOffset)
		{
			return HttpRestProcessor.GetRangeFromRequest(HttpRequestAccessorCommon.GetRangeHeaderValue(headers), allowUnspecifiedEndOffset, out startOffset, out endOffset);
		}

		public static bool GetRangeFromRequest(string rangeHeaderValue, bool allowUnspecifiedEndOffset, out long startOffset, out long endOffset)
		{
			long num = (long)0;
			long num1 = num;
			endOffset = num;
			startOffset = num1;
			if (string.IsNullOrEmpty(rangeHeaderValue))
			{
				return false;
			}
			string[] strArrays = rangeHeaderValue.Split(HeaderValues.RangeHeaderSeparatorForUnitAndRangeSplitter, 3, StringSplitOptions.None);
			if ((int)strArrays.Length != 2)
			{
				Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log("Invalid range value. It must contain only one {0}", new object[] { "=" });
				return false;
			}
			if (string.Compare(strArrays[0], "bytes", StringComparison.OrdinalIgnoreCase) != 0)
			{
				IStringDataEventStream infoDebug = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
				object[] objArray = new object[] { strArrays[0] };
				infoDebug.Log("Byte range unit {0} is not supported", objArray);
				return false;
			}
			string str = strArrays[1];
			string[] strArrays1 = str.Split(HeaderValues.HeaderSeparatorForStartAndEndInRangesSplitter, 3, StringSplitOptions.None);
			if ((int)strArrays1.Length != 2)
			{
				Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log("Byte range specifier {0} must have only one {1}", new object[] { str, "-" });
				return false;
			}
			if (!long.TryParse(strArrays1[0], out startOffset) || startOffset < (long)0)
			{
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
				object[] objArray1 = new object[] { strArrays1[0] };
				stringDataEventStream.Log("Can't convert start offset {0} or it's negative", objArray1);
				return false;
			}
			if (!allowUnspecifiedEndOffset || !string.IsNullOrEmpty(strArrays1[1]))
			{
				if (!long.TryParse(strArrays1[1], out endOffset) || endOffset < (long)0)
				{
					IStringDataEventStream infoDebug1 = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
					object[] objArray2 = new object[] { strArrays1[1] };
					infoDebug1.Log("Can't convert end offset {0} or it's negative", objArray2);
					return false;
				}
				if (startOffset > endOffset)
				{
					IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.InfoDebug;
					object[] objArray3 = new object[] { startOffset, endOffset };
					stringDataEventStream1.Log("Start offset {0} is greater than end offset {1}", objArray3);
					return false;
				}
			}
			else
			{
				endOffset = (long)-1;
				Logger<IRestProtocolHeadLogger>.Instance.InfoDebug.Log("Allowing end offset {0} to be unspecified.");
			}
			return true;
		}

		protected bool GetRangeFromRequestAndAdjustEndOffset(NameValueCollection requestHeaders, bool allowUnspecifiedEndOffset, out long startOffset, out long endOffset, out bool isEndOffsetSpecified)
		{
			isEndOffsetSpecified = true;
			bool rangeFromRequest = HttpRestProcessor.GetRangeFromRequest(requestHeaders, allowUnspecifiedEndOffset, out startOffset, out endOffset);
			if (allowUnspecifiedEndOffset && endOffset == (long)-1 || !rangeFromRequest)
			{
				endOffset = 9223372036854775807L;
				isEndOffsetSpecified = false;
			}
			endOffset = this.PreventOverflowForRange(rangeFromRequest, startOffset, endOffset);
			return rangeFromRequest;
		}

		private void GetSequenceNumberConditionFromRequest(out ComparisonOperator? comparisonOperator, out long? sequenceNumber)
		{
			comparisonOperator = null;
			sequenceNumber = null;
			string item = base.RequestHeadersCollection["x-ms-if-sequence-number-lt"];
			string str = base.RequestHeadersCollection["x-ms-if-sequence-number-le"];
			string item1 = base.RequestHeadersCollection["x-ms-if-sequence-number-eq"];
			string str1 = null;
			string str2 = null;
			if (!string.IsNullOrEmpty(item))
			{
				if (!string.IsNullOrEmpty(str))
				{
					throw new InvalidHeaderProtocolException("x-ms-if-sequence-number-le", base.RequestHeadersCollection["x-ms-if-sequence-number-le"]);
				}
				if (!string.IsNullOrEmpty(item1))
				{
					throw new InvalidHeaderProtocolException("x-ms-if-sequence-number-eq", base.RequestHeadersCollection["x-ms-if-sequence-number-eq"]);
				}
				comparisonOperator = new ComparisonOperator?(ComparisonOperator.LessThan);
				str1 = item;
				str2 = "x-ms-if-sequence-number-lt";
			}
			else if (string.IsNullOrEmpty(str))
			{
				if (string.IsNullOrEmpty(item1))
				{
					return;
				}
				comparisonOperator = new ComparisonOperator?(ComparisonOperator.Equal);
				str1 = item1;
				str2 = "x-ms-if-sequence-number-eq";
			}
			else
			{
				if (!string.IsNullOrEmpty(item1))
				{
					throw new InvalidHeaderProtocolException("x-ms-if-sequence-number-eq", base.RequestHeadersCollection["x-ms-if-sequence-number-eq"]);
				}
				comparisonOperator = new ComparisonOperator?(ComparisonOperator.LessThanOrEqual);
				str1 = str;
				str2 = "x-ms-if-sequence-number-le";
			}
			try
			{
				sequenceNumber = new long?(long.Parse(str1));
			}
			catch (FormatException formatException)
			{
				throw new InvalidHeaderProtocolException(str2, str1, formatException);
			}
			NephosAssertionException.Assert(comparisonOperator.HasValue, "comparisonOperator must have a value at this point!");
			NephosAssertionException.Assert(sequenceNumber.HasValue, "sequenceNumber must have a value at this point!");
		}

		private long? GetSequenceNumberFromRequest()
		{
			long? nullable = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-blob-sequence-number", null);
			if (nullable.HasValue)
			{
				long? nullable1 = nullable;
				if ((nullable1.GetValueOrDefault() >= (long)0 ? false : nullable1.HasValue))
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-sequence-number", base.RequestHeadersCollection["x-ms-blob-sequence-number"]);
				}
			}
			return nullable;
		}

		private SequenceNumberUpdate GetSequenceNumberUpdateFromRequest()
		{
			string item = base.RequestHeadersCollection["x-ms-sequence-number-action"];
			SequenceNumberUpdateType sequenceNumberUpdateType = SequenceNumberUpdateType.None;
			long? sequenceNumberFromRequest = this.GetSequenceNumberFromRequest();
			if (string.IsNullOrEmpty(item))
			{
				if (sequenceNumberFromRequest.HasValue)
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-sequence-number-action", string.Format("The header {0} is required with the header {1}.", "x-ms-sequence-number-action", "x-ms-blob-sequence-number"));
				}
				return null;
			}
			if (item.Equals("update", StringComparison.OrdinalIgnoreCase))
			{
				if (!sequenceNumberFromRequest.HasValue)
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-blob-sequence-number", string.Format("The header {0} is required when header {1} is {2}.", "x-ms-blob-sequence-number", "x-ms-sequence-number-action", "update"));
				}
				sequenceNumberUpdateType = SequenceNumberUpdateType.Update;
			}
			else if (!item.Equals("max", StringComparison.OrdinalIgnoreCase))
			{
				if (!item.Equals("increment", StringComparison.OrdinalIgnoreCase))
				{
					throw new InvalidHeaderProtocolException("x-ms-sequence-number-action", item);
				}
				if (sequenceNumberFromRequest.HasValue)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-sequence-number", base.RequestHeadersCollection["x-ms-blob-sequence-number"]);
				}
				sequenceNumberUpdateType = SequenceNumberUpdateType.Increment;
				sequenceNumberFromRequest = new long?((long)1);
			}
			else
			{
				if (!sequenceNumberFromRequest.HasValue)
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-blob-sequence-number", string.Format("The header {0} is required when header {1} is {2}.", "x-ms-blob-sequence-number", "x-ms-sequence-number-action", "max"));
				}
				sequenceNumberUpdateType = SequenceNumberUpdateType.Max;
			}
			NephosAssertionException.Assert(sequenceNumberFromRequest.HasValue);
			return new SequenceNumberUpdate(sequenceNumberUpdateType, sequenceNumberFromRequest.Value);
		}

		protected DateTime? GetSnapshot(bool isDoingVersionCheck, bool ensureIsRootBlobRequest, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ParameterSource parameterSource, string parameterName)
		{
			if (isDoingVersionCheck && !base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				return null;
			}
			DateTime? nullable = base.ParseOptionalDateTimeInput((BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource)parameterSource, parameterName, true);
			if (!nullable.HasValue)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("No snapshot; this request is for the root blob.");
			}
			else
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] str = new object[] { nullable.Value.ToString("HH:mm:ss.fffffff") };
				verbose.Log("Found snapshot '{0}'", str);
				if (ensureIsRootBlobRequest)
				{
					throw new InvalidQueryParameterProtocolException("snapshot", base.RequestQueryParameters["snapshot"], "This operation is only allowed on the root blob. Snapshot should not be provided.");
				}
				if (!string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-lease-id"]))
				{
					throw new InvalidQueryParameterProtocolException("snapshot", base.RequestQueryParameters["snapshot"], "Leases are not allowed with snapshots.");
				}
			}
			return nullable;
		}

		private IEnumerator<IAsyncResult> IncrementalCopyBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			string str;
			Logger<IRestProtocolHeadLogger>.Instance.Info.Log("IncrementalCopyBlobImpl");
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(HttpRestProcessor.DefaultMaxAllowedTimeoutForCopyBlob);
			this.EnsureIsRootBlobRequest();
			Guid? leaseId = this.GetLeaseId(false);
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			NephosAssertionException.Assert(this.operationContext.ResourceIsBlobOrFilePath);
			NephosAssertionException.Assert(!string.IsNullOrEmpty(base.RequestCopySource));
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromIncrementalCopyRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.CopyOperation, base.RequestRestVersion);
			bool flag = false;
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable = leaseId;
			ComparisonOperator? nullable1 = null;
			long? nullable2 = null;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable, nullable1, nullable2, BlobType.None, flag, false, false, false, nullable3, null, nullable4);
			OverwriteOption overwriteOption = base.GetOverwriteOption(this.operationContext.RequestConditionInformation, leaseId);
			UriString uriString = new UriString(base.RequestCopySource);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			verbose.Log("RequestIncrementalCopySource is: {0}", new object[] { uriString.SafeStringForLogging });
			Uri uri = null;
			if (uriString.RawString.Length > 2048)
			{
				throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
			}
			try
			{
				uri = new Uri(uriString.RawString);
			}
			catch (UriFormatException uriFormatException)
			{
				throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging, uriFormatException);
			}
			if (!uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) && !uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Copy source scheme is not http[s].");
				throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
			}
			try
			{
				MetadataEncoding.Validate(uriString.RawString);
			}
			catch (FormatException formatException1)
			{
				FormatException formatException = formatException1;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] message = new object[] { formatException.Message };
				throw new MetadataFormatException(string.Format(invariantCulture, "Invalid Copy-source contains non-ASCII character, Exception:{0}", message), formatException);
			}
			bool isUriPathStyle = base.IsUriPathStyle;
			NephosUriComponents nephosUriComponent = null;
			IAsyncResult asyncResult = base.BeginGetUriComponents(uri, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), async.GetResumeCallback(), async.GetResumeState("ServiceManager.BeginGetUriComponents"));
			yield return asyncResult;
			try
			{
				nephosUriComponent = base.EndGetUriComponents(asyncResult);
				NephosAssertionException.Assert(nephosUriComponent != null, "GetSourceUriComponents completed but returned null");
			}
			catch (InvalidUrlProtocolException invalidUrlProtocolException1)
			{
				InvalidUrlProtocolException invalidUrlProtocolException = invalidUrlProtocolException1;
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				stringDataEventStream.Log("BeginGetUriComponents on copy source failed with {0}. The source is not an Azure blob.", new object[] { invalidUrlProtocolException.Message });
				throw new InvalidSourceBlobUrlException();
			}
			DateTime? nullable5 = null;
			string item = HttpUtilities.GetQueryParameters(uri)["snapshot"];
			if (item == null)
			{
				IStringDataEventStream verbose1 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				verbose1.Log("The copy source is missing snapshot time. The copy source is '{0}'.", new object[] { uriString.SafeStringForLogging });
				throw new IncrementalCopySourceMustBeSnapshotException();
			}
			if (!HttpUtilities.TryGetSnapshotDateTimeFromHttpString(item, out nullable5))
			{
				IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] safeStringForLogging = new object[] { item, uriString.SafeStringForLogging };
				stringDataEventStream1.Log("The copy source is invalid because the snapshot specified in the copy source is invalid. The snapshot string is '{0}' and the copy source is '{1}'.", safeStringForLogging);
				throw new InvalidHeaderProtocolException("x-ms-copy-source", uriString.SafeStringForLogging);
			}
			IStringDataEventStream verbose2 = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			verbose2.Log("Snapshot present in copy source: '{0}'.", new object[] { item });
			IncrementalCopyBlobMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as IncrementalCopyBlobMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			operationMeasurementEvent.SourceUrl = uriString.RawString;
			operationMeasurementEvent.DestinationAccountName = this.operationContext.AccountName;
			operationMeasurementEvent.DestinationContainerName = this.operationContext.ContainerName;
			operationMeasurementEvent.DestinationBlobName = this.operationContext.BlobOrFilePathName;
			ICopyBlobResult copyBlobResult = null;
			bool flag1 = false;
			IAsyncResult asyncResult1 = this.serviceManager.BeginAsynchronousCopyBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, uriString, false, flag1, FECopyType.Incremental, nullable5, new CopyBlobProperties(), blobObjectCondition, overwriteOption, null, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("this.serviceManager.AsynchronousCopyBlob"));
			yield return asyncResult1;
			TimeSpan zero = TimeSpan.Zero;
			try
			{
				copyBlobResult = this.serviceManager.EndAsynchronousCopyBlob(asyncResult1, out zero);
			}
			finally
			{
				T timeSpan = this.operationContext;
				TimeSpan operationInternetRequestRoundTripLatency = this.operationContext.OperationInternetRequestRoundTripLatency;
				timeSpan.OperationInternetRequestRoundTripLatency = new TimeSpan(operationInternetRequestRoundTripLatency.Ticks + zero.Ticks);
			}
			this.AddLastModifiedAndETagToResponse(copyBlobResult.LastModifiedTime);
			this.AddCopyInfoToResponse(copyBlobResult.CopyInfo);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private bool IsGetBlobOperation(RestMethod method)
		{
			if (!this.operationContext.ResourceIsBlobOrFilePath || this.operationContext.SubResource != null)
			{
				return false;
			}
			return method == RestMethod.GET;
		}

		protected bool IsRequestUrlSignedAccess(Uri request)
		{
			bool flag = false;
			flag = (!request.Query.Contains("?sig=") ? request.Query.Contains("&sig=") : true);
			return flag;
		}

		protected bool IsSpecifiedRangeValidForCalculatingCrc64OrMd5(bool? isCrc64OrMd5Requested, string headerName, bool isRangeSpecified, bool isEndOffsetSpecified, long length, long maxAllowedRangeLengthForCalculation)
		{
			if (!isRangeSpecified || !isEndOffsetSpecified)
			{
				if (isCrc64OrMd5Requested.HasValue)
				{
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("The header {0} is only allowed with ranged gets and a range was not specified.", new object[] { headerName });
					throw new InvalidHeaderProtocolException(headerName, base.RequestHeadersCollection[headerName]);
				}
				return false;
			}
			if (!isCrc64OrMd5Requested.HasValue || !isCrc64OrMd5Requested.Value)
			{
				return false;
			}
			if (length > maxAllowedRangeLengthForCalculation)
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] objArray = new object[] { headerName, maxAllowedRangeLengthForCalculation };
				verbose.Log("The header {0} is only allowed if the range length is <= {1} bytes.", objArray);
				throw new InvalidHeaderProtocolException(headerName, base.RequestHeadersCollection[headerName]);
			}
			return true;
		}

		private IEnumerator<IAsyncResult> ListBlobsImpl(AsyncIteratorContext<NoResults> asyncContext)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.GetDefaultMaxTimeoutForListCommands(base.RequestRestVersion));
			bool containerName = this.operationContext.ContainerName == null;
			ListBlobsOperationContext listBlobsOperationContext = new ListBlobsOperationContext(base.RequestRestVersion);
			this.FillListBlobsOperationContext(listBlobsOperationContext, containerName);
			if (listBlobsOperationContext.IsIncludingSnapshots && !string.IsNullOrEmpty(listBlobsOperationContext.Delimiter))
			{
				throw new InvalidQueryParameterProtocolException("delimiter", listBlobsOperationContext.Delimiter, "Delimiter and including snapshots are mutually exclusive.");
			}
			BlobServiceVersion blobServiceVersion = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobServiceVersion.Sept09 : BlobServiceVersion.None);
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				blobServiceVersion = BlobServiceVersion.Feb12;
			}
			IAsyncResult stream = this.serviceManager.BeginListBlobs(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, blobServiceVersion, listBlobsOperationContext.Prefix, listBlobsOperationContext.Delimiter, listBlobsOperationContext.ContainerMarkerListingBlobs, listBlobsOperationContext.Marker, listBlobsOperationContext.SnapshotMarker, listBlobsOperationContext.IncludeIfModifiedSince, listBlobsOperationContext.IncludeIfNotModifiedSince, listBlobsOperationContext.IncludeDisabledContainers, listBlobsOperationContext.IsFetchingMetadata, listBlobsOperationContext.IsIncludingSnapshots, listBlobsOperationContext.IsIncludingPageBlobs, listBlobsOperationContext.IsIncludingAppendBlobs, listBlobsOperationContext.IsIncludingUncommittedBlobs, listBlobsOperationContext.IsIncludingLeaseStatusInResponse, listBlobsOperationContext.MaxResults, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, asyncContext.GetResumeCallback(), asyncContext.GetResumeState("HttpRestProcessor.ListBlobsImpl"));
			yield return stream;
			IListBlobsResultCollection listBlobsResultCollections = this.serviceManager.EndListBlobs(stream);
			bool flag = false;
			using (listBlobsResultCollections)
			{
				base.Response.SendChunked = true;
				base.Response.ContentType = "application/xml";
				this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
				ListBlobsXmlListEncoder listBlobsXmlListEncoder = new ListBlobsXmlListEncoder(false, flag);
				NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as ListBlobsMeasurementEvent != null);
				using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
				{
					if (this.operationContext.ContainerName != null)
					{
						NephosAssertionException.Assert(!listBlobsOperationContext.ListingAcrossContainers, "lboc.ListingAcrossContainers must be false when the passed in ContainerName is not null");
					}
					else
					{
						NephosAssertionException.Assert(listBlobsOperationContext.ListingAcrossContainers, "lboc.ListingAcrossContainers must be true when the passed in ContainerName is null");
					}
					stream = listBlobsXmlListEncoder.BeginEncodeListToStream(base.GetRequestUrlHidingNonStandardPorts(), listBlobsResultCollections, listBlobsOperationContext, stream1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), asyncContext.GetResumeCallback(), asyncContext.GetResumeState("HttpRestProcessor.ListBlobsImpl"));
					yield return stream;
					listBlobsXmlListEncoder.EndEncodeListToStream(stream);
					this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = listBlobsXmlListEncoder.TotalCount;
				}
			}
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ListContainersImpl(AsyncIteratorContext<NoResults> asyncContext)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.GetDefaultMaxTimeoutForListCommands(base.RequestRestVersion));
			ListContainersOperationContext listContainersOperationContext = new ListContainersOperationContext(base.RequestRestVersion);
			this.FillListBaseObjectsOperationContext(listContainersOperationContext);
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				List<string> strs = base.ParseOptionalListInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.QueryString, "include", HttpRestProcessor.ValidListContainersIncludeQueryParamValues, ",");
				if (strs != null && strs.Count > 0)
				{
					foreach (string str in strs)
					{
						if (!str.Equals("metadata", StringComparison.OrdinalIgnoreCase))
						{
							throw new InvalidQueryParameterProtocolException("include", str, "Invalid query parameter value.");
						}
						listContainersOperationContext.IsFetchingMetadata = true;
					}
				}
			}
			if (listContainersOperationContext.Delimiter != null)
			{
				throw new UnknownQueryParameterProtocolException("delimiter");
			}
			if (!string.IsNullOrEmpty(listContainersOperationContext.Prefix) && Comparison.StringContains(listContainersOperationContext.Prefix, "/"))
			{
				throw new InvalidQueryParameterProtocolException("prefix", listContainersOperationContext.Prefix, "Cannot contain slash.");
			}
			DateTime? nullable = null;
			DateTime? nullable1 = null;
			IAsyncResult stream = this.serviceManager.BeginListContainers(this.operationContext.CallerIdentity, this.operationContext.AccountName, listContainersOperationContext.Prefix, null, listContainersOperationContext.Marker, listContainersOperationContext.IncludeDisabledContainers, false, listContainersOperationContext.IsIncludingLeaseStateAndDurationInResponse, nullable, nullable1, listContainersOperationContext.MaxResults, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, asyncContext.GetResumeCallback(), asyncContext.GetResumeState("HttpRestProcessor.ListContainersImpl"));
			yield return stream;
			IListContainersResultCollection listContainersResultCollections = this.serviceManager.EndListContainers(stream);
			base.Response.SendChunked = true;
			base.Response.ContentType = "application/xml";
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			NephosAssertionException.Assert(this.operationContext.OperationMeasurementEvent as ListContainersMeasurementEvent != null);
			using (Stream stream1 = base.GenerateMeasuredResponseStream(false))
			{
				ListContainersXmlListEncoder listContainersXmlListEncoder = new ListContainersXmlListEncoder(base.RequestContext.IsRequestVersionAtLeastAugust11);
				stream = listContainersXmlListEncoder.BeginEncodeListToStream(base.GetRequestUrlHidingNonStandardPorts(), listContainersResultCollections, listContainersOperationContext, stream1, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), asyncContext.GetResumeCallback(), asyncContext.GetResumeState("HttpRestProcessor.ListCollectionsImpl"));
				yield return stream;
				listContainersXmlListEncoder.EndEncodeListToStream(stream);
				this.operationContext.OperationMeasurementEvent.ItemsReturnedCount = listContainersXmlListEncoder.TotalCount;
			}
			base.SendSuccessResponse(false);
		}

		protected override void PerformCommonRestWork()
		{
			base.PerformCommonRestWork();
		}

		protected long PreventOverflowForRange(bool isRangeSpecified, long startOffset, long endOffset)
		{
			if (isRangeSpecified && endOffset == 9223372036854775807L && startOffset != 9223372036854775807L)
			{
				endOffset -= (long)1;
			}
			return endOffset;
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			bool flag;
			bool flag1;
			bool flag2;
			bool flag3;
			string str;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			PutBlobMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutBlobMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			NephosAssertionException.Assert(this.operationContext.ResourceIsBlobOrFilePath);
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength < (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			if (!string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-access-tier"]))
			{
				throw new HeaderNotSupportedProtocolException("x-ms-access-tier");
			}
			PutBlobProperties putBlobPropertiesFromRequest = this.GetPutBlobPropertiesFromRequest();
			operationMeasurementEvent.BlobType = putBlobPropertiesFromRequest.BlobType;
			bool flag4 = false;
			if (!base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				NephosAssertionException.Assert(putBlobPropertiesFromRequest.BlobType == BlobType.ListBlob);
			}
			else if (putBlobPropertiesFromRequest.BlobType != BlobType.IndexBlob)
			{
				if (putBlobPropertiesFromRequest.BlobType == BlobType.ListBlob && putBlobPropertiesFromRequest.MaxBlobSize.HasValue)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-content-length", base.RequestHeadersCollection["x-ms-blob-content-length"]);
				}
				if (putBlobPropertiesFromRequest.BlobType == BlobType.AppendBlob)
				{
					if (base.RequestContentLength != (long)0)
					{
						IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
						object[] objArray = new object[] { base.RequestContentLength };
						verbose.Log("Failing the PutBlob since ContentLength is not equal to 0 for an AppendBlob (value is {0}).", objArray);
						throw new InvalidHeaderProtocolException("Content-Length", base.RequestHeadersCollection["Content-Length"]);
					}
					if (putBlobPropertiesFromRequest.MaxBlobSize.HasValue)
					{
						throw new InvalidHeaderProtocolException("x-ms-blob-content-length", base.RequestHeadersCollection["x-ms-blob-content-length"]);
					}
				}
			}
			else
			{
				long num = 1099511627776L;
				if (!putBlobPropertiesFromRequest.MaxBlobSize.HasValue)
				{
					throw new RequiredHeaderNotPresentProtocolException("x-ms-blob-content-length");
				}
				if (putBlobPropertiesFromRequest.MaxBlobSize.Value < (long)0 || putBlobPropertiesFromRequest.MaxBlobSize.Value > num)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-content-length", base.RequestHeadersCollection["x-ms-blob-content-length"]);
				}
				if (putBlobPropertiesFromRequest.MaxBlobSize.Value % (long)512 != (long)0)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-content-length", base.RequestHeadersCollection["x-ms-blob-content-length"]);
				}
				if (base.RequestContentLength != (long)0)
				{
					IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] requestContentLength1 = new object[] { base.RequestContentLength };
					stringDataEventStream.Log("Failing the PutBlob since ContentLength is not equal to 0 for a PageBlob (value is {0}).", requestContentLength1);
					throw new InvalidHeaderProtocolException("Content-Length", base.RequestHeadersCollection["Content-Length"]);
				}
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			OverwriteOption overwriteOption = base.GetOverwriteOption(this.operationContext.RequestConditionInformation, nullable);
			operationMeasurementEvent.BlobSize = base.RequestContentLength;
			this.operationContext.MaxAllowedTimeout = BlobObjectHelper.GetSizeBasedTimeout(base.RequestContentLength, TimeSpan.MaxValue, this.MinimumSizeBasedTimeout, ServiceConstants.DefaultPutDataRateForTimeout);
			base.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
			this.operationContext.IsSizeBased = true;
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable1, nullable2, nullable3, BlobType.None, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable4);
			IPutBlobResult putBlobResult = null;
			using (Stream stream = base.GenerateMeasuredRequestStream())
			{
				bool flag5 = true;
				bool supportCrc64 = base.SupportCrc64;
				flag = (!putBlobPropertiesFromRequest.ContentCrc64.HasValue ? false : putBlobPropertiesFromRequest.BlobType == BlobType.ListBlob);
				bool flag6 = flag;
				if (putBlobPropertiesFromRequest.BlobType != BlobType.ListBlob || putBlobPropertiesFromRequest.ContentMD5 == null)
				{
					flag1 = false;
				}
				else
				{
					flag1 = (!base.SupportCrc64 ? true : !putBlobPropertiesFromRequest.ContentCrc64.HasValue);
				}
				bool flag7 = flag1;
				if (!base.SupportCrc64)
				{
					flag2 = true;
				}
				else if (!base.SupportCrc64 || putBlobPropertiesFromRequest.ContentCrc64.HasValue)
				{
					flag2 = false;
				}
				else if (putBlobPropertiesFromRequest.ContentMD5 != null)
				{
					flag2 = true;
				}
				else
				{
					bool? putBlobComputeMD5 = putBlobPropertiesFromRequest.PutBlobComputeMD5;
					flag2 = (!putBlobComputeMD5.GetValueOrDefault() ? false : putBlobComputeMD5.HasValue);
				}
				bool flag8 = flag2;
				if (!base.SupportCrc64 && (base.RequestContext.IsRequestVersionAtLeastFebruary12 || putBlobPropertiesFromRequest.ContentMD5 != null))
				{
					flag3 = true;
				}
				else if (!base.SupportCrc64)
				{
					flag3 = false;
				}
				else if (putBlobPropertiesFromRequest.ContentMD5 != null)
				{
					flag3 = true;
				}
				else
				{
					bool? putBlobComputeMD51 = putBlobPropertiesFromRequest.PutBlobComputeMD5;
					flag3 = (!putBlobComputeMD51.GetValueOrDefault() ? false : putBlobComputeMD51.HasValue);
				}
				bool flag9 = flag3;
				bool flag10 = false;
				bool flag11 = false;
				if (base.RequestContext.IsRequestVersionAtLeastMay16)
				{
					flag11 = true;
					if (base.RequestContentLength > (long)268435456)
					{
						throw new BlobContentTooLargeException(new long?((long)268435456), null, null);
					}
				}
				IAsyncResult asyncResult = this.serviceManager.BeginPutBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, stream, base.RequestContentLength, putBlobPropertiesFromRequest, base.SupportCrc64, flag5, supportCrc64, flag6, flag8, flag9, flag7, flag10, flag11, flag4, blobObjectCondition, overwriteOption, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.PutBlobImpl"));
				yield return asyncResult;
				putBlobResult = this.serviceManager.EndPutBlob(asyncResult);
			}
			if (putBlobPropertiesFromRequest.BlobType == BlobType.ListBlob)
			{
				if (base.SupportCrc64)
				{
					this.AddNephosContentCrc64ToResponse(putBlobResult.ContentCrc64);
				}
				if (base.SupportCrc64)
				{
					if (!base.SupportCrc64)
					{
						goto Label0;
					}
					else if (putBlobPropertiesFromRequest.ContentMD5 == null)
					{
						bool? putBlobComputeMD52 = putBlobPropertiesFromRequest.PutBlobComputeMD5;
						if ((!putBlobComputeMD52.GetValueOrDefault() ? true : !putBlobComputeMD52.HasValue))
						{
							goto Label0;
						}
					}
				}
				NephosAssertionException.Assert(putBlobResult.ContentMD5 != null, "PutBlob should return ContentMD5 for BlockBlob if: 1. CRC64 is not supported 2. CRC64 is supported and Content-MD5 exists or x-ms-put-blob-compute-md5 header is true");
				this.AddContentMD5ToResponse(putBlobResult.ContentMD5, "Content-MD5");
			}
		Label0:
			this.AddLastModifiedAndETagToResponse(putBlobResult.LastModifiedTime);
			this.AddRequestServerEncryptionStatusToResponse(putBlobResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> PutBlockImpl(AsyncIteratorContext<NoResults> async)
		{
			long? requestCrc64;
			string str;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			PutBlockMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutBlockMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			if (!string.IsNullOrEmpty(HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str)))
			{
				throw new HeaderNotSupportedProtocolException(str);
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			bool flag = false;
			if (base.RequestContext.IsRequestVersionAtLeastMay16)
			{
				flag = true;
				if (base.RequestContentLength > (long)104857600)
				{
					throw new BlobContentTooLargeException(new long?((long)104857600), null, null);
				}
			}
			this.EnsureCrc64AndMd5HeaderAreMutuallyExclusive(false);
			string item = base.RequestQueryParameters["blockid"];
			if (string.IsNullOrEmpty(item))
			{
				throw new NullOrEmptyQueryParameterProtocolException("blockid");
			}
			byte[] numArray = null;
			try
			{
				numArray = Convert.FromBase64String(item);
			}
			catch (FormatException formatException)
			{
				throw new InvalidQueryParameterProtocolException("blockid", item, "Not a valid base64 string.", formatException);
			}
			if (numArray == null || (int)numArray.Length == 0)
			{
				throw new InvalidQueryParameterProtocolException("blockid", item, "Not a valid base64 string.");
			}
			operationMeasurementEvent.BlockSize = base.RequestContentLength;
			this.operationContext.MaxAllowedTimeout = BlobObjectHelper.GetSizeBasedTimeout(base.RequestContentLength, TimeSpan.MaxValue, this.MinimumSizeBasedTimeout, ServiceConstants.DefaultPutDataRateForTimeout);
			base.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
			ArchiveBlobContext archiveBlobContext = new ArchiveBlobContext();
			HttpRestProcessor httpRestProcessor = this;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(null, nullable1, nullable2, nullable3, BlobType.ListBlob, false, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.IsOperationAllowedOnArchivedBlobs, archiveBlobContext.LMT, archiveBlobContext.GenerationId, nullable4);
			IPutBlockResult putBlockResult = null;
			using (Stream stream = base.GenerateMeasuredRequestStream())
			{
				Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
				IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
				string accountName = this.operationContext.AccountName;
				string containerName = this.operationContext.ContainerName;
				string blobOrFilePathName = this.operationContext.BlobOrFilePathName;
				byte[] numArray1 = numArray;
				Stream stream1 = stream;
				long num = base.RequestContentLength;
				if (base.SupportCrc64)
				{
					requestCrc64 = base.GetRequestCrc64("x-ms-content-crc64");
				}
				else
				{
					requestCrc64 = null;
				}
				IAsyncResult asyncResult = serviceManager.BeginPutBlock(callerIdentity, accountName, containerName, blobOrFilePathName, numArray1, stream1, num, requestCrc64, base.GetRequestMD5("Content-MD5"), flag, base.SupportCrc64, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.PutBlockImpl"));
				yield return asyncResult;
				putBlockResult = this.serviceManager.EndPutBlock(asyncResult);
			}
			this.AddContentCrc64Md5ToResponse(putBlockResult);
			this.AddRequestServerEncryptionStatusToResponse(putBlockResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> PutPageImpl(AsyncIteratorContext<NoResults> async)
		{
			long? requestCrc64;
			string str;
			long num;
			long num1;
			ComparisonOperator? nullable;
			long? nullable1;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			this.EnsureIsRootBlobRequest();
			Guid? nullable2 = this.GetLeaseId(false);
			PutPageMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as PutPageMeasurementEvent;
			NephosAssertionException.Assert(operationMeasurementEvent != null);
			string str1 = HttpRequestAccessorCommon.GetRangeHeaderValue(base.RequestHeadersCollection, out str);
			if (string.IsNullOrEmpty(str1))
			{
				throw new RequiredHeaderNotPresentProtocolException("Range");
			}
			if (string.IsNullOrEmpty(base.RequestHeadersCollection["Content-Length"]))
			{
				throw new RequiredHeaderNotPresentProtocolException("Content-Length");
			}
			if (base.RequestContentLength <= (long)0)
			{
				long requestContentLength = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
			}
			this.EnsureCrc64AndMd5HeaderAreMutuallyExclusive(false);
			this.Range = str1;
			if (!HttpRestProcessor.GetRangeFromRequest(str1, out num, out num1))
			{
				throw new InvalidHeaderProtocolException("Range", base.RequestHeadersCollection[str]);
			}
			this.EnsureRangeIsPageAligned(num, num1, str1);
			long num2 = num1 - num + (long)1;
			if (base.RequestContext.IsRequestVersionAtLeastAugust13 && base.RequestContentLength != num2)
			{
				long requestContentLength1 = base.RequestContentLength;
				throw new InvalidHeaderProtocolException("Content-Length", requestContentLength1.ToString(CultureInfo.InvariantCulture));
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			this.GetSequenceNumberConditionFromRequest(out nullable, out nullable1);
			operationMeasurementEvent.DataSize = base.RequestContentLength;
			this.operationContext.MaxAllowedTimeout = BlobObjectHelper.GetSizeBasedTimeout(base.RequestContentLength, TimeSpan.MaxValue, this.MinimumSizeBasedTimeout, ServiceConstants.DefaultPutDataRateForTimeout);
			base.EnsureMaxTimeoutIsNotExceeded(this.operationContext.MaxAllowedTimeout);
			this.operationContext.IsSizeBased = true;
			bool flag = false;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable2, nullable, nullable1, BlobType.IndexBlob, flag, false, false, false, nullable3, null, nullable4);
			IPutPageResult putPageResult = null;
			using (Stream stream = base.GenerateMeasuredRequestStream())
			{
				Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
				IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
				string accountName = this.operationContext.AccountName;
				string containerName = this.operationContext.ContainerName;
				string blobOrFilePathName = this.operationContext.BlobOrFilePathName;
				Stream stream1 = stream;
				long num3 = num;
				long num4 = num1 - num + (long)1;
				if (base.SupportCrc64)
				{
					requestCrc64 = base.GetRequestCrc64("x-ms-content-crc64");
				}
				else
				{
					requestCrc64 = null;
				}
				IAsyncResult asyncResult = serviceManager.BeginPutPage(callerIdentity, accountName, containerName, blobOrFilePathName, stream1, num3, num4, requestCrc64, base.GetRequestMD5("Content-MD5"), base.SupportCrc64, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.PutPageImpl"));
				yield return asyncResult;
				putPageResult = this.serviceManager.EndPutPage(asyncResult);
			}
			this.AddRequestServerEncryptionStatusToResponse(putPageResult.IsWriteEncrypted);
			this.AddContentCrc64Md5ToResponse(putPageResult);
			this.AddLastModifiedAndETagToResponse(putPageResult.LastModifiedTime);
			this.AddSequenceNumberToResponse(putPageResult.SequenceNumber);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ReleaseBlobLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Guid? nullable1 = null;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			DateTime? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable1, nullable2, nullable3, BlobType.None, false, false, true, false, nullable4, null, nullable5);
			IAsyncResult asyncResult = this.serviceManager.BeginReleaseBlobLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nullable.Value, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.ReleaseBlobLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndReleaseBlobLease(asyncResult);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> ReleaseContainerLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ContainerCondition containerCondition = BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ConvertToContainerCondition(this.operationContext.RequestConditionInformation);
			IAsyncResult asyncResult = this.serviceManager.BeginReleaseContainerLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, nullable.Value, containerCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.ReleaseContainerLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndReleaseContainerLease(asyncResult);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> RenewBlobLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Guid? nullable1 = null;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			DateTime? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable1, nullable2, nullable3, BlobType.None, false, false, true, false, nullable4, null, nullable5);
			IAsyncResult asyncResult = this.serviceManager.BeginRenewBlobLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, LeaseType.ReadWrite, nullable.Value, HttpRestProcessor.DefaultLeaseDuration, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.RenewBlobLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndRenewBlobLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> RenewContainerLeaseImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = this.GetLeaseId(true);
			NephosAssertionException.Assert(nullable.HasValue, "Expected lease ID to have a value or for a protocol exception to be thrown.");
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ContainerCondition containerCondition = BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.ConvertToContainerCondition(this.operationContext.RequestConditionInformation);
			IAsyncResult asyncResult = this.serviceManager.BeginRenewContainerLease(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, LeaseType.ReadWrite, nullable.Value, HttpRestProcessor.DefaultLeaseDuration, containerCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.RenewContainerLeaseImpl"));
			yield return asyncResult;
			ILeaseInfoResult leaseInfoResult = this.serviceManager.EndRenewContainerLease(asyncResult);
			this.SetLeaseInfoOnResponse(leaseInfoResult.LeaseInfo, HttpRestProcessor.LeaseResultOptions.Id);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				this.AddLastModifiedAndETagToResponse(leaseInfoResult.LastModifiedTime);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private void RethrowBlobErrorForDoubleLookupIfNecessary()
		{
			if (this.operationContext.BlobErrorToRethrowForDoubleLookup != null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Rethrowing blob error for double lookup.");
				throw this.operationContext.BlobErrorToRethrowForDoubleLookup;
			}
		}

		private TimeSpan SafeTimeSpanAdd(TimeSpan time, TimeSpan timeToAdd)
		{
			if ((TimeSpan.MaxValue - time) <= timeToAdd)
			{
				return TimeSpan.MaxValue;
			}
			return time + timeToAdd;
		}

		protected void SendCacheSuccessResponse(bool hit)
		{
			base.SendSuccessResponse((hit ? NephosRESTEventStatus.SuccessCacheHit : NephosRESTEventStatus.Success), false);
		}

		private IEnumerator<IAsyncResult> SetBlobMetadataImpl(AsyncIteratorContext<NoResults> async)
		{
			BlobType blobType;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			bool flag = false;
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			blobType = (base.RequestContext.IsRequestVersionAtLeastSeptember09 ? BlobType.None : BlobType.ListBlob);
			DateTime? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable1, nullable2, nullable3, blobType, flag, false, false, false, nullable4, null, nullable5);
			IAsyncResult asyncResult = this.serviceManager.BeginSetBlobMetadata(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nameValueCollection, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetBlobMetadataImpl"));
			yield return asyncResult;
			ISetBlobMetadataResult setBlobMetadataResult = this.serviceManager.EndSetBlobMetadata(asyncResult);
			this.AddLastModifiedAndETagToResponse(setBlobMetadataResult.LastModifiedTime);
			this.AddRequestServerEncryptionStatusToResponse(setBlobMetadataResult.IsWriteEncrypted);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetBlobPropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			long? requestCrc64;
			object obj;
			object obj1;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			if (!string.IsNullOrEmpty(base.RequestHeadersCollection["x-ms-access-tier"]))
			{
				throw new HeaderNotSupportedProtocolException("x-ms-access-tier");
			}
			NephosAssertionException.Assert(this.operationContext.ResourceIsBlobOrFilePath);
			PutBlobProperties putBlobProperty = new PutBlobProperties()
			{
				CacheControl = base.RequestHeadersCollection["x-ms-blob-cache-control"],
				ContentEncoding = base.RequestHeadersCollection["x-ms-blob-content-encoding"],
				ContentLanguage = base.RequestHeadersCollection["x-ms-blob-content-language"],
				ContentType = base.RequestHeadersCollection["x-ms-blob-content-type"]
			};
			PutBlobProperties putBlobProperty1 = putBlobProperty;
			if (base.SupportCrc64)
			{
				requestCrc64 = base.GetRequestCrc64("x-ms-blob-content-crc64");
			}
			else
			{
				requestCrc64 = null;
			}
			putBlobProperty1.ContentCrc64 = requestCrc64;
			putBlobProperty.ContentMD5 = base.GetRequestMD5("x-ms-blob-content-md5");
			putBlobProperty.MaxBlobSize = base.ParseOptionalLongInput(BasicHttpProcessorWithAuthAndAccountContainer<T>.ParameterSource.HeaderValue, "x-ms-blob-content-length", null);
			putBlobProperty.SequenceNumberUpdate = this.GetSequenceNumberUpdateFromRequest();
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] blobOrFilePathName = new object[] { this.operationContext.BlobOrFilePathName, null, null };
			object[] objArray = blobOrFilePathName;
			obj = (putBlobProperty.ContentMD5 != null ? base.RequestHeadersCollection["x-ms-blob-content-md5"] : "Empty");
			objArray[1] = obj;
			object[] objArray1 = blobOrFilePathName;
			obj1 = (putBlobProperty.ContentCrc64.HasValue ? base.RequestHeadersCollection["x-ms-content-crc64"] : "Empty");
			objArray1[2] = obj1;
			verbose.Log("UserMD5andCRC64: BlobName: {0}, MD5: {1}, CRC64: {2}", blobOrFilePathName);
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				putBlobProperty.ContentDisposition = base.RequestHeadersCollection["x-ms-blob-content-disposition"];
			}
			long num = 1099511627776L;
			if (putBlobProperty.MaxBlobSize.HasValue && (putBlobProperty.MaxBlobSize.Value < (long)0 || putBlobProperty.MaxBlobSize.Value > num || putBlobProperty.MaxBlobSize.Value % (long)512 != (long)0))
			{
				throw new InvalidHeaderProtocolException("x-ms-blob-content-length", base.RequestHeadersCollection["x-ms-blob-content-length"]);
			}
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			bool flag = false;
			HttpRestProcessor httpRestProcessor = this;
			ConditionInformation requestConditionInformation = this.operationContext.RequestConditionInformation;
			Guid? nullable1 = nullable;
			ComparisonOperator? nullable2 = null;
			long? nullable3 = null;
			DateTime? nullable4 = null;
			Guid? nullable5 = null;
			BlobObjectCondition blobObjectCondition = httpRestProcessor.ConvertToBlobObjectCondition(requestConditionInformation, nullable1, nullable2, nullable3, BlobType.None, flag, false, false, false, nullable4, null, nullable5);
			IAsyncResult asyncResult = this.serviceManager.BeginSetBlobProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, putBlobProperty, base.SupportCrc64, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetBlobPropertiesImpl"));
			yield return asyncResult;
			ISetBlobPropertiesResult setBlobPropertiesResult = this.serviceManager.EndSetBlobProperties(asyncResult);
			this.AddLastModifiedAndETagToResponse(setBlobPropertiesResult.LastModifiedTime);
			if (setBlobPropertiesResult.SequenceNumber.HasValue)
			{
				this.AddSequenceNumberToResponse(setBlobPropertiesResult.SequenceNumber.Value);
			}
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		protected void SetBlobPropertiesOnResponse(IBlobProperties blobProperties, bool isWritingMD5, bool isWritingAcceptRanges, bool isWriteNephosMD5Header, bool excludeNonSystemHeaders, bool includeInternalProperties, bool obfuscateSourceUri)
		{
			if (blobProperties.BlobMetadata != null && !excludeNonSystemHeaders)
			{
				base.SetMetadataOnResponse(blobProperties.BlobMetadata);
			}
			base.Response.ContentType = blobProperties.ContentType;
			base.Response.ContentLength64 = blobProperties.ContentLength;
			if (!excludeNonSystemHeaders)
			{
				if (blobProperties.ContentEncoding != null)
				{
					base.Response.AddHeader("Content-Encoding", blobProperties.ContentEncoding);
				}
				if (blobProperties.ContentLanguage != null)
				{
					base.Response.AddHeader("Content-Language", blobProperties.ContentLanguage);
				}
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && blobProperties.CacheControl != null)
				{
					base.Response.AddHeader("Cache-Control", blobProperties.CacheControl);
				}
			}
			IBlobContentMeasurementEvent operationMeasurementEvent = this.operationContext.OperationMeasurementEvent as IBlobContentMeasurementEvent;
			if (operationMeasurementEvent != null)
			{
				operationMeasurementEvent.ContentEncoding = blobProperties.ContentEncoding;
				operationMeasurementEvent.ContentLanguage = blobProperties.ContentLanguage;
				if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
				{
					operationMeasurementEvent.CacheControl = blobProperties.CacheControl;
				}
			}
			if (blobProperties.LastModifiedTime.HasValue)
			{
				this.AddLastModifiedAndETagToResponse(blobProperties.LastModifiedTime.Value);
			}
			if (base.SupportCrc64 && blobProperties.ContentCrc64.HasValue)
			{
				this.AddNephosContentCrc64ToResponse(blobProperties.ContentCrc64);
			}
			if (blobProperties.ContentMD5 != null)
			{
				if (isWritingMD5)
				{
					this.AddContentMD5ToResponse(blobProperties.ContentMD5, "Content-MD5");
				}
				if (isWriteNephosMD5Header)
				{
					this.AddContentMD5ToResponse(blobProperties.ContentMD5, "x-ms-blob-content-md5");
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && blobProperties.LeaseInfo != null)
			{
				this.AddLeaseStatusToResponse(blobProperties.LeaseInfo);
			}
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09)
			{
				base.Response.Headers["x-ms-blob-type"] = BlobTypeStrings.GetString(blobProperties.BlobType);
			}
			if (blobProperties.BlobType == BlobType.AppendBlob && blobProperties.CommittedBlockCount.HasValue)
			{
				base.Response.Headers["x-ms-blob-committed-block-count"] = blobProperties.CommittedBlockCount.Value.ToString();
			}
			if (base.RequestContext.IsRequestVersionAtLeastSeptember09 && blobProperties.SequenceNumber.HasValue)
			{
				base.Response.Headers["x-ms-blob-sequence-number"] = blobProperties.SequenceNumber.ToString();
			}
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12 && !excludeNonSystemHeaders)
			{
				if (blobProperties.CopyId != null)
				{
					base.Response.Headers["x-ms-copy-id"] = blobProperties.CopyId;
				}
				if (blobProperties.CopySource != null)
				{
					base.Response.Headers["x-ms-copy-source"] = (obfuscateSourceUri ? HttpUtilities.ObfuscateSourceUri(blobProperties.CopySource) : blobProperties.CopySource);
				}
				if (blobProperties.CopyStatus != null)
				{
					base.Response.Headers["x-ms-copy-status"] = blobProperties.CopyStatus;
				}
				if (blobProperties.CopyStatusDescription != null)
				{
					base.Response.Headers["x-ms-copy-status-description"] = blobProperties.CopyStatusDescription;
				}
				if (blobProperties.CopyProgress != null)
				{
					base.Response.Headers["x-ms-copy-progress"] = blobProperties.CopyProgress;
				}
				if (blobProperties.CopyCompletionTime.HasValue && blobProperties.CopyStatus != null && !blobProperties.CopyStatus.Equals("pending", StringComparison.OrdinalIgnoreCase))
				{
					WebHeaderCollection headers = base.Response.Headers;
					DateTime? copyCompletionTime = blobProperties.CopyCompletionTime;
					headers["x-ms-copy-completion-time"] = HttpUtilities.ConvertDateTimeToHttpString(copyCompletionTime.Value);
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastMay16 && blobProperties.IsIncrementalCopy)
			{
				base.Response.Headers["x-ms-incremental-copy"] = "true";
				if (blobProperties.LastCopySnapshot.HasValue && blobProperties.CopyStatus != null && blobProperties.CopyStatus.Equals("success", StringComparison.OrdinalIgnoreCase))
				{
					if (blobProperties.LastCopySnapshot.Value <= DateTimeConstants.MinimumIncrementalCopySnapshotTime)
					{
						AlertsManager.AlertOrLogException("LastCopySnapshot set to Minimum value, while a valid timestamp was expected.", null, null);
					}
					WebHeaderCollection httpString = base.Response.Headers;
					DateTime? lastCopySnapshot = blobProperties.LastCopySnapshot;
					httpString["x-ms-copy-destination-snapshot"] = HttpUtilities.ConvertSnapshotDateTimeToHttpString(lastCopySnapshot.Value);
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastAugust13)
			{
				if (!excludeNonSystemHeaders && blobProperties.ContentDisposition != null)
				{
					base.Response.AddHeader("Content-Disposition", blobProperties.ContentDisposition);
					operationMeasurementEvent.ContentDisposition = blobProperties.ContentDisposition;
				}
				if (!string.IsNullOrEmpty(base.SASVersion))
				{
					this.AddQueryResponseHeadersForSas();
				}
			}
			if (base.RequestContext.IsRequestVersionAtLeastDecember15)
			{
				base.Response.AddHeader("x-ms-server-encrypted", (!blobProperties.IsBlobEncrypted.HasValue || !blobProperties.IsBlobEncrypted.Value ? "false" : "true"));
			}
			if (isWritingAcceptRanges)
			{
				base.Response.AddHeader("Accept-Ranges", "bytes");
			}
			if (excludeNonSystemHeaders)
			{
				base.Response.AddHeader("x-ms-exclude-non-system-headers", excludeNonSystemHeaders.ToString().ToLower());
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Excluding non system headers from the response");
			}
		}

		private IEnumerator<IAsyncResult> SetBlobServicePropertiesImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			IAsyncResult asyncResult = base.BeginReadAnalyticsSettings(AnalyticsSettingsHelper.GetSettingVersion(base.RequestContext), base.RequestContext.ServiceType, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetBlobServicePropertiesImpl"));
			yield return asyncResult;
			AnalyticsSettings analyticsSetting = base.EndReadAnalyticsSettings(asyncResult);
			asyncResult = this.serviceManager.BeginSetBlobServiceProperties(this.operationContext.CallerIdentity, this.operationContext.AccountName, analyticsSetting, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetBlobServicePropertiesImpl"));
			yield return asyncResult;
			this.serviceManager.EndSetBlobServiceProperties(asyncResult);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Accepted);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetContainerAclImpl(AsyncIteratorContext<NoResults> async)
		{
			DateTime? ifModifiedSince;
			DateTime? ifNotModifiedSince;
			ContainerAclSettings containerAclSetting;
			IAsyncResult asyncResult;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId(false);
			}
			base.CreateContainerAclSettingsFromHeader(true, out containerAclSetting);
			if (!base.RequestContext.IsRequestVersionAtLeastJuly09)
			{
				DateTime? nullable1 = null;
				DateTime? nullable2 = null;
				Guid? nullable3 = null;
				asyncResult = this.serviceManager.BeginSetContainerAcl(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, containerAclSetting, nullable1, nullable2, nullable3, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetContainerAclImpl"));
				yield return asyncResult;
			}
			else
			{
				if (base.RequestContentLength > (long)0)
				{
					if (base.RequestContentLength > (long)10000)
					{
						throw new RequestEntityTooLargeException(new long?((long)10000));
					}
					using (BufferPoolMemoryStream bufferPoolMemoryStream = new BufferPoolMemoryStream(8192))
					{
						using (Stream stream = base.GenerateMeasuredRequestStream())
						{
							IAsyncResult asyncResult1 = AsyncStreamCopy.BeginAsyncStreamCopy(stream, bufferPoolMemoryStream, base.RequestContentLength, 8192, this.operationContext.RemainingTimeout(), async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetContainerImpl"));
							yield return asyncResult1;
							AsyncStreamCopy.EndAsyncStreamCopy(asyncResult1);
						}
						bufferPoolMemoryStream.Seek((long)0, SeekOrigin.Begin);
						SASPermission sASPermission = SASPermission.Blob;
						if (base.RequestContext.IsRequestVersionAtLeastApril15)
						{
							sASPermission = SASPermission.BlobWithAddAndCreate;
						}
						containerAclSetting.SASIdentifiers = BasicHttpProcessor.DecodeSASIdentifiersFromStream(bufferPoolMemoryStream, BasicHttpProcessorWithAuthAndAccountContainer<OperationContext>.DefaultXmlReaderSettings, true, base.RequestContext.IsRequestVersionAtLeastApril15, sASPermission);
						if (containerAclSetting.SASIdentifiers.Count <= SASIdentifier.MaxSASIdentifiers)
						{
							goto Label0;
						}
						throw new InvalidXmlProtocolException(string.Concat("At most ", SASIdentifier.MaxSASIdentifiers, " signed identifier is allowed in the request body"));
					}
				}
				else if (base.RequestContentLength < (long)0)
				{
					long requestContentLength = base.RequestContentLength;
					throw new InvalidHeaderProtocolException("Content-Length", requestContentLength.ToString(CultureInfo.InvariantCulture));
				}
			Label0:
				this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
				Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
				IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
				string accountName = this.operationContext.AccountName;
				string containerName = this.operationContext.ContainerName;
				ContainerAclSettings containerAclSetting1 = containerAclSetting;
				if (this.operationContext.RequestConditionInformation != null)
				{
					ifModifiedSince = this.operationContext.RequestConditionInformation.IfModifiedSince;
				}
				else
				{
					ifModifiedSince = null;
				}
				if (this.operationContext.RequestConditionInformation != null)
				{
					ifNotModifiedSince = this.operationContext.RequestConditionInformation.IfNotModifiedSince;
				}
				else
				{
					ifNotModifiedSince = null;
				}
				asyncResult = serviceManager.BeginSetContainerAcl(callerIdentity, accountName, containerName, containerAclSetting1, ifModifiedSince, ifNotModifiedSince, nullable, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetContainerAclImpl"));
				yield return asyncResult;
			}
			this.AddLastModifiedAndETagToResponse(this.serviceManager.EndSetContainerAcl(asyncResult));
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		private IEnumerator<IAsyncResult> SetContainerMetadataImpl(AsyncIteratorContext<NoResults> async)
		{
			DateTime? ifModifiedSince;
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			Guid? nullable = null;
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12)
			{
				nullable = this.GetLeaseId(false);
			}
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager serviceManager = this.serviceManager;
			IAccountIdentifier callerIdentity = this.operationContext.CallerIdentity;
			string accountName = this.operationContext.AccountName;
			string containerName = this.operationContext.ContainerName;
			NameValueCollection nameValueCollection1 = nameValueCollection;
			if (this.operationContext.RequestConditionInformation != null)
			{
				ifModifiedSince = this.operationContext.RequestConditionInformation.IfModifiedSince;
			}
			else
			{
				ifModifiedSince = null;
			}
			IAsyncResult asyncResult = serviceManager.BeginSetContainerMetadata(callerIdentity, accountName, containerName, nameValueCollection1, ifModifiedSince, nullable, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SetContainerMetadataImpl"));
			yield return asyncResult;
			this.AddLastModifiedAndETagToResponse(this.serviceManager.EndSetContainerMetadata(asyncResult));
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.OK);
			base.SendSuccessResponse(false);
		}

		protected void SetContainerPropertiesOnResponse(IContainerProperties properties)
		{
			if (properties.ContainerMetadata != null)
			{
				base.SetMetadataOnResponse(properties.ContainerMetadata);
			}
			this.AddLastModifiedAndETagToResponse(properties.LastModifiedTime);
			if (base.RequestContext.IsRequestVersionAtLeastFebruary12 && base.RequestContext.ServiceType == Microsoft.Cis.Services.Nephos.Common.ServiceType.BlobService && properties.LeaseInfo != null)
			{
				this.AddLeaseStatusToResponse(properties.LeaseInfo);
			}
		}

		private void SetLeaseInfoOnResponse(ILeaseInfo leaseInfo, HttpRestProcessor.LeaseResultOptions leaseResultOptions)
		{
			if ((leaseResultOptions & HttpRestProcessor.LeaseResultOptions.Id) != HttpRestProcessor.LeaseResultOptions.None && leaseInfo.Id.HasValue)
			{
				IHttpListenerResponse response = base.Response;
				Guid value = leaseInfo.Id.Value;
				response.AddHeader("x-ms-lease-id", value.ToString("D"));
			}
			if ((leaseResultOptions & HttpRestProcessor.LeaseResultOptions.Duration) != HttpRestProcessor.LeaseResultOptions.None && leaseInfo.Duration.HasValue)
			{
				int totalSeconds = (int)leaseInfo.Duration.Value.TotalSeconds;
				base.Response.AddHeader("x-ms-lease-time", totalSeconds.ToString());
			}
			if ((leaseResultOptions & HttpRestProcessor.LeaseResultOptions.EndTime) != HttpRestProcessor.LeaseResultOptions.None && leaseInfo.EndTime.HasValue)
			{
				string httpString = HttpUtilities.ConvertDateTimeToHttpString(leaseInfo.EndTime.Value);
				base.Response.AddHeader("x-ms-lease-end-time", httpString);
			}
		}

		private bool ShouldExcludeNonSystemHeaders()
		{
			return false;
		}

		private bool ShouldIncludeInternalProperties()
		{
			return false;
		}

		private IEnumerator<IAsyncResult> SnapshotBlobImpl(AsyncIteratorContext<NoResults> async)
		{
			this.EnsureRequestHttpVersionIsSupported(HttpRestProcessor.DefaultNonSupportedHttpVersions);
			base.EnsureMaxTimeoutIsNotExceeded(BasicHttpProcessor.DefaultMaxAllowedTimeout);
			this.EnsureIsRootBlobRequest();
			Guid? nullable = this.GetLeaseId(false);
			NameValueCollection nameValueCollection = new NameValueCollection();
			base.AddMetadataFromRequest(nameValueCollection);
			this.operationContext.RequestConditionInformation = ConditionExtractor.GetConditionInfoFromRequest(base.RequestHeadersCollection, OperationTypeForConditionParsing.WriteOperation, base.RequestRestVersion);
			ComparisonOperator? nullable1 = null;
			long? nullable2 = null;
			DateTime? nullable3 = null;
			Guid? nullable4 = null;
			BlobObjectCondition blobObjectCondition = base.ConvertToBlobObjectCondition(this.operationContext.RequestConditionInformation, nullable, nullable1, nullable2, BlobType.None, false, false, false, false, nullable3, null, nullable4);
			IAsyncResult asyncResult = this.serviceManager.BeginSnapshotBlob(this.operationContext.CallerIdentity, this.operationContext.AccountName, this.operationContext.ContainerName, this.operationContext.BlobOrFilePathName, nameValueCollection, blobObjectCondition, this.operationContext.OperationDuration.Remaining(this.operationContext.OperationTimeout), base.RequestContext, async.GetResumeCallback(), async.GetResumeState("HttpRestProcessor.SnapshotBlobImpl"));
			yield return asyncResult;
			ISnapshotBlobResult snapshotBlobResult = this.serviceManager.EndSnapshotBlob(asyncResult);
			this.AddSnapshotTimestampToResponse(snapshotBlobResult.SnapshotTimestamp);
			this.AddLastModifiedAndETagToResponse(snapshotBlobResult.LastModifiedTime);
			this.SetStatusCodeAndServiceHeaders(HttpStatusCode.Created);
			base.SendSuccessResponse(false);
		}

		private void ThrowBlobErrorForDoubleLoookupIfNecessary(ContainerAclSettings containerAcl)
		{
			string publicAccessLevel;
			if (containerAcl != null)
			{
				publicAccessLevel = containerAcl.PublicAccessLevel;
			}
			else
			{
				publicAccessLevel = null;
			}
			string str = publicAccessLevel;
			if (this.operationContext.BlobErrorToRethrowForDoubleLookup != null && !string.IsNullOrEmpty(str) && (str.Equals("container", StringComparison.OrdinalIgnoreCase) || str.Equals("blob", StringComparison.OrdinalIgnoreCase)))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Rethrowing blob error for double lookup since container has a new ACL.");
				throw this.operationContext.BlobErrorToRethrowForDoubleLookup;
			}
		}

		protected override void ValidateAndAdjustContext(OperationContext context, NephosUriComponents uriComponents, bool suppressException)
		{
			if (base.RequestContext.ServiceType == Microsoft.Cis.Services.Nephos.Common.ServiceType.FileService && context.IsResourceTypeDirectory && !string.IsNullOrEmpty(uriComponents.ContainerName))
			{
				uriComponents.RemainingPart = uriComponents.RemainingPart ?? string.Empty;
			}
			else if (base.RequestContext.IsRequestVersionAtLeastJuly09 && !context.IsResourceTypeContainer)
			{
				if (!string.IsNullOrEmpty(uriComponents.AccountName) && !string.IsNullOrEmpty(uriComponents.ContainerName) && uriComponents.IsRemainingPartPresentButEmpty && !suppressException)
				{
					bool flag = string.IsNullOrEmpty(uriComponents.RemainingPart);
					object[] str = new object[] { uriComponents.ToString() };
					NephosAssertionException.Assert(flag, "uriComponents.IsRemainingPartPresentButEmpty was true, but the remaining part is not empty. URI components = {0}", str);
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Empty blob names are not allowed. URI Components: {0}", new object[] { uriComponents });
					throw new InvalidUrlProtocolException(base.RequestUrl);
				}
				if (!string.IsNullOrEmpty(uriComponents.AccountName) && !string.IsNullOrEmpty(uriComponents.ContainerName) && string.IsNullOrEmpty(uriComponents.RemainingPart) && !suppressException)
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] requestRestVersion = new object[] { base.RequestRestVersion, base.IsRequestAnonymousAndUnversioned, uriComponents };
					verbose.Log("Adjusting for root container since request is version {0} (IsRequestAnonymousAndUnversioned = {1}) and resourcetype=container was not given. URI Components before adjustment: {2}", requestRestVersion);
					uriComponents.AdjustForRootContainer();
				}
			}
			context.AccountName = uriComponents.AccountName;
			context.ContainerName = uriComponents.ContainerName;
			context.BlobOrFilePathName = uriComponents.RemainingPart;
			this.ValidateUri(context, uriComponents);
		}

		private void ValidateAppendBlockConditionArguments(long? conditionalMaxBlobSize, long? conditionalAppendPosition)
		{
			if (conditionalMaxBlobSize.HasValue)
			{
				long value = conditionalMaxBlobSize.Value;
				if (value <= (long)0)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-condition-maxsize", value.ToString());
				}
			}
			if (conditionalAppendPosition.HasValue)
			{
				long num = conditionalAppendPosition.Value;
				if (num < (long)0 || num >= 209715200000L)
				{
					throw new InvalidHeaderProtocolException("x-ms-blob-condition-appendpos", num.ToString());
				}
			}
		}

		private void ValidateUri(OperationContext context, NephosUriComponents uriComponents)
		{
			if (!base.RequestContext.IsRequestVersionAtLeastJuly09 && context.IsRequestUsingRootContainer)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Root container is only allowed for July09+ versions.");
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			if (context.IsRequestUsingRootContainer && base.RequestContext.ServiceType != Microsoft.Cis.Services.Nephos.Common.ServiceType.FileService && !string.IsNullOrEmpty(context.BlobOrFilePathName) && context.BlobOrFilePathName.Contains("/"))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Forward slash is not allowed for blobs under the root container.");
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			if (base.RequestContext.IsRequestVersionAtLeastJuly09 && context.IsResourceTypeContainer && !string.IsNullOrEmpty(context.BlobOrFilePathName))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Blob name is not allowed when the resource type is container. URI Components: {0}", new object[] { uriComponents });
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			if (!string.IsNullOrEmpty(context.BlobOrFilePathName) && (string.IsNullOrEmpty(context.ContainerName) || string.IsNullOrEmpty(context.AccountName)))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("If the blob name is present, both account and container name must also be present. URI Components: {0}", new object[] { uriComponents });
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
			if (!string.IsNullOrEmpty(context.ContainerName) && string.IsNullOrEmpty(context.AccountName))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("If the container name is present, the account must also be present. URI Components: {0}", new object[] { uriComponents });
				throw new InvalidUrlProtocolException(base.RequestUrl);
			}
		}

		[Flags]
		public enum LeaseResultOptions
		{
			None = 0,
			Id = 1,
			Duration = 2,
			EndTime = 4
		}
	}
}