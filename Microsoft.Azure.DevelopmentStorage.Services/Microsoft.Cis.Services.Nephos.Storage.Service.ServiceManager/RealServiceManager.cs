using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Service;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using Microsoft.Cis.Services.Nephos.Common.Versioning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class RealServiceManager : Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager
	{
		private readonly static TimeSpan MaxAllowedTimeoutToGetResponseFromCopySource;

		public static TimeSpan LeaseDurationInfinite;

		public readonly static string ContentEncodingTag;

		public readonly static string ContentLanguageTag;

		public readonly static string ContentCrc64Tag;

		public readonly static string ContentMD5Tag;

		public readonly static string EmptyContentMD5Value;

		public readonly static string CacheControlTag;

		public readonly static string ContentDispositionTag;

		public readonly static string CopyIdTag;

		public readonly static string CopySourceTag;

		public readonly static string CopyProgressTotalTag;

		public readonly static string CopyTypeTag;

		public readonly static string LastCopySnapshotTag;

		public readonly static string CopyProgressDiskSizeTag;

		public readonly static string WriteProtection;

		public readonly static string DiskTag;

		public readonly static string DiskResourceUri;

		public readonly static string CopyStatusTag;

		public readonly static string CopyStatusDescriptionTag;

		public readonly static string CopyProgressOffsetTag;

		public readonly static string CopyCompletionTimeTag;

		public readonly static string XSmbContainerQuotaMetadataName;

		public readonly static int MinShareQuotaInGB;

		public readonly static int MaxShareQuotaInGBPriorToLargeFileShareFeature;

		[SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes", Justification="config is intended to be readonly, even though the underlying type is not immutable.")]
		protected readonly ServiceManagerConfiguration config;

		private static int ServicePropertiesSizeThreshold;

		public override Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		static RealServiceManager()
		{
			RealServiceManager.MaxAllowedTimeoutToGetResponseFromCopySource = TimeSpan.FromSeconds(30);
			RealServiceManager.LeaseDurationInfinite = TimeSpan.FromSeconds(-1);
			RealServiceManager.ContentEncodingTag = "ContentEncoding";
			RealServiceManager.ContentLanguageTag = "ContentLanguage";
			RealServiceManager.ContentCrc64Tag = "ContentCrc64";
			RealServiceManager.ContentMD5Tag = "ContentMD5";
			RealServiceManager.EmptyContentMD5Value = "EmptyMD5";
			RealServiceManager.CacheControlTag = "CacheControl";
			RealServiceManager.ContentDispositionTag = "ContentDisposition";
			RealServiceManager.CopyIdTag = "CopyId";
			RealServiceManager.CopySourceTag = "CopySource";
			RealServiceManager.CopyProgressTotalTag = "CopyProgressTotal";
			RealServiceManager.CopyTypeTag = "CopyType";
			RealServiceManager.LastCopySnapshotTag = "LastCopySnapshot";
			RealServiceManager.CopyProgressDiskSizeTag = "CopyProgressDiskSize";
			RealServiceManager.WriteProtection = "WriteProtection";
			RealServiceManager.DiskTag = "DiskTag";
			RealServiceManager.DiskResourceUri = "DiskResourceUri";
			RealServiceManager.CopyStatusTag = "CopyStatus";
			RealServiceManager.CopyStatusDescriptionTag = "CopyStatusDescription";
			RealServiceManager.CopyProgressOffsetTag = "CopyProgressOffset";
			RealServiceManager.CopyCompletionTimeTag = "CopyCompletionTime";
			RealServiceManager.XSmbContainerQuotaMetadataName = "XSmbContainerQuota";
			RealServiceManager.MinShareQuotaInGB = 1;
			RealServiceManager.MaxShareQuotaInGBPriorToLargeFileShareFeature = 5120;
			RealServiceManager.ServicePropertiesSizeThreshold = 8192;
		}

		protected RealServiceManager(AuthorizationManager authorizationManager, IStorageManager storageManager, ServiceManagerConfiguration config)
		{
			if (authorizationManager == null)
			{
				throw new ArgumentNullException("authorizationManager");
			}
			if (storageManager == null)
			{
				throw new ArgumentNullException("storageManager");
			}
			if (config == null)
			{
				throw new ArgumentNullException("config");
			}
			this.config = config;
			this.authorizationManager = authorizationManager;
			this.storageManager = storageManager;
		}

		private IEnumerator<IAsyncResult> AbortCopyBlobImpl(IAccountIdentifier identifier, string account, string container, string blob, Guid copyId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in AbortCopyBlob");
			}
			if (!requestContext.IsRequestVersionAtLeastApril15)
			{
				asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
				yield return asyncResult;
				this.authorizationManager.EndCheckAccess(asyncResult);
			}
			else
			{
				SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
				{
					SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
					SignedResourceType = SasResourceType.Object,
					SignedPermission = SASPermission.Write
				};
				SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
				asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
				yield return asyncResult;
				this.authorizationManager.EndCheckAccess(asyncResult);
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginAbortCopyBlob(copyId, condition, context.GetResumeCallback(), context.GetResumeState("destBlobObject.BeginAbortCopyBlob"));
					yield return asyncResult;
					blobObject.EndAbortCopyBlob(asyncResult);
				}
			}
		}

		private IEnumerator<IAsyncResult> AcquireBlobLeaseImpl(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in AcquireBlobLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.AcquireBlobLeaseImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginAcquireLease(leaseType, leaseDuration, proposedLeaseId, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.AcquireBlobLeaseImpl"));
					yield return asyncResult;
					blobObject.EndAcquireLease(asyncResult);
					ILeaseInfo leaseInfo = blobObject.LeaseInfo;
					NephosAssertionException.Assert(leaseInfo != null, "AcquireBlobLease call for blob {0} completed successfully, but the lease info is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					Guid? id = leaseInfo.Id;
					NephosAssertionException.Assert(id.HasValue, "AcquireBlobLease call for blob {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					TimeSpan? nullable = leaseInfo.Duration;
					NephosAssertionException.Assert(nullable.HasValue, "AcquireBlobLease call for blob {0} completed successfully, but the lease duration is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "AcquireBlobLease call for blob {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value,
						LeaseInfo = leaseInfo
					};
					context.ResultData = leaseInfoResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> AcquireContainerLeaseImpl(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in AcquireContainerLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginAcquireLease(leaseType, leaseDuration, proposedLeaseId, condition, !requestContext.IsRequestVersionAtLeastAugust13, requestContext.IsRequestVersionAtLeastMay16, context.GetResumeCallback(), context.GetResumeState("IBlobContainer.BeginAcquireLease"));
				yield return asyncResult;
				blobContainer.EndAcquireLease(asyncResult);
				ILeaseInfo leaseInfo = blobContainer.LeaseInfo;
				NephosAssertionException.Assert(leaseInfo != null, "AcquireContainerLease call for container {0} completed successfully, but the lease info is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				Guid? id = leaseInfo.Id;
				NephosAssertionException.Assert(id.HasValue, "AcquireContainerLease call for container {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "AcquireContainerLease call for container {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
				{
					LastModifiedTime = blobContainer.LastModificationTime.Value,
					LeaseInfo = leaseInfo
				};
				context.ResultData = leaseInfoResult;
			}
		}

		private static void AddCopySourceServiceMetadata(string resource, NameValueCollection serviceMetadataCollection, string name, string value)
		{
			try
			{
				RealServiceManager.AddServiceProperty(resource, serviceMetadataCollection, name, value);
			}
			catch (MetadataFormatException metadataFormatException1)
			{
				MetadataFormatException metadataFormatException = metadataFormatException1;
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("FE CopyBlob: MetadataFormatException while validating service metadata: {0}", new object[] { metadataFormatException.Message });
				string str = string.Format("Copy source {0} contains invalid property {1} with value {2}.", resource, name, value);
				throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, str, metadataFormatException);
			}
		}

		protected static void AddServiceProperty(string resource, NameValueCollection serviceProperties, string name, string value)
		{
			if (value != null)
			{
				try
				{
					MetadataEncoding.Validate(value);
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { name, value, resource, formatException.Message };
					throw new MetadataFormatException(string.Format(invariantCulture, "Invalid {0} with value '{1}' for blob {2}: {3}", objArray), formatException);
				}
				serviceProperties.Add(name, value);
			}
		}

		private TimeSpan AdjustTimeoutBasedOnSizeForGetBlob(long blobLength, BlobRegion blobRegion, TimeSpan userTimeout)
		{
			long num = blobLength;
			if (blobRegion != null)
			{
				num = Math.Min(blobRegion.Length, blobLength - blobRegion.Offset);
			}
			TimeSpan sizeBasedTimeout = BlobObjectHelper.GetSizeBasedTimeout(num, userTimeout, BlobObjectHelper.MinGetBlobTimeout, BlobObjectHelper.GetDataRateForTimeout);
			return sizeBasedTimeout;
		}

		public static void AlertIfServicePropertiesExceedSizeThreshold(string contentType, NameValueCollection serviceMetadataCollection)
		{
			int length = 0;
			if (serviceMetadataCollection != null)
			{
				string[] allKeys = serviceMetadataCollection.AllKeys;
				for (int i = 0; i < (int)allKeys.Length; i++)
				{
					string str = allKeys[i];
					length = length + str.Length + 1 + serviceMetadataCollection[str].Length + Environment.NewLine.Length;
				}
			}
			RealServiceManager.AlertIfServicePropertiesExceedSizeThreshold(contentType, null, length);
		}

		public static void AlertIfServicePropertiesExceedSizeThreshold(string contentType, byte[] serviceMetadata)
		{
			RealServiceManager.AlertIfServicePropertiesExceedSizeThreshold(contentType, serviceMetadata, 0);
		}

		private static void AlertIfServicePropertiesExceedSizeThreshold(string contentType, byte[] serviceMetadata, int serviceMetadataFallbackSize)
		{
			int num = (contentType != null ? contentType.Length : 0);
			int num1 = (serviceMetadata != null ? (int)serviceMetadata.Length : serviceMetadataFallbackSize);
			int num2 = num1 + num;
			if (num2 > 8192)
			{
				IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] objArray = new object[] { num, num1, num2 };
				error.Log("FE ServiceProperties: ContentType + ServiceMetadata greater than current 8KB character limit: {0} + {1} = {2}", objArray);
			}
			if (num2 > RealServiceManager.ServicePropertiesSizeThreshold)
			{
				RealServiceManager.ServicePropertiesSizeThreshold = num2;
				Logger<IRestProtocolHeadLogger>.Instance.Critical.Log("FE ServiceProperties: size hit new maximum of {0} characters", new object[] { num2 });
			}
		}

		private IEnumerator<IAsyncResult> AppendBlockImpl(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool supportCRC64, BlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IAppendBlockResult> context)
		{
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (inputStream == null)
			{
				throw new ArgumentNullException("inputStream");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in AppendBlock");
			}
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Add
			};
			this.authorizationManager.CheckAccessWithMultiplePermissions(identifier, account, container, blob, PermissionLevel.Write, SasType.ResourceSas | SasType.AccountSas, SasResourceType.Object, sASPermissions, startingNow.Remaining(timeout));
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IListBlobObject listBlobObject = blobContainer.CreateAppendBlobInstance(blob))
				{
					Stream stream = inputStream;
					CrcReaderStream crcReaderStream = null;
					MD5ReaderStream mD5ReaderStream = null;
					if (contentMD5 != null)
					{
						mD5ReaderStream = new MD5ReaderStream(stream, contentLength, false, requestContext)
						{
							HashToVerifyAgainst = contentMD5
						};
						stream = mD5ReaderStream;
					}
					else if (!supportCRC64)
					{
						mD5ReaderStream = new MD5ReaderStream(stream, contentLength, false, requestContext);
						stream = mD5ReaderStream;
					}
					try
					{
						listBlobObject.Timeout = startingNow.Remaining(timeout);
						IAsyncResult asyncResult = listBlobObject.BeginAppendBlock(contentLength, stream, crcReaderStream, condition, conditionalMaxBlobSize, conditionalAppendBlockPosition, context.GetResumeCallback(), context.GetResumeState("IListBlobObject.AppendBlock"));
						yield return asyncResult;
						listBlobObject.EndAppendBlock(asyncResult);
						AppendBlockResult hash = new AppendBlockResult();
						AppendBlockResult appendBlockResult = hash;
						flag = (listBlobObject.IsWriteEncrypted.HasValue ? listBlobObject.IsWriteEncrypted.Value : false);
						appendBlockResult.IsWriteEncrypted = flag;
						try
						{
							if (!supportCRC64)
							{
								hash.ContentMD5 = mD5ReaderStream.Hash;
							}
							else
							{
								if (contentCRC64.HasValue)
								{
									hash.ContentCrc64 = new long?(crcReaderStream.CalculatedCrc);
								}
								if (contentMD5 != null)
								{
									hash.ContentMD5 = mD5ReaderStream.Hash;
								}
							}
						}
						catch (InvalidOperationException invalidOperationException1)
						{
							InvalidOperationException invalidOperationException = invalidOperationException1;
							string str = "AppendBlock call for blob {0} completed successfully, however CRC64 or MD5 hash of blob contents is not available, which indicates that not all of the blob contents were read during the AppendBlock call.";
							object[] resourceString = new object[] { RealServiceManager.GetResourceString(account, container, blob) };
							NephosAssertionException.Fail(invalidOperationException, str, resourceString);
						}
						hash.LastModifiedTime = listBlobObject.LastModificationTime.Value;
						hash.AppendOffset = listBlobObject.BlobAppendOffset.Value;
						hash.CommittedBlockCount = listBlobObject.CommittedBlockCount.Value;
						context.ResultData = hash;
					}
					finally
					{
						if (mD5ReaderStream != null)
						{
							((IDisposable)mD5ReaderStream).Dispose();
						}
						if (crcReaderStream != null)
						{
							((IDisposable)crcReaderStream).Dispose();
						}
					}
				}
			}
		}

		protected void ApplyAuthorizationConditions(Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.AuthorizationCondition authorizationCondition, IBaseObjectCondition condition, ref OverwriteOption overwriteOption)
		{
			if (authorizationCondition != null && authorizationCondition.MustNotExist)
			{
				if ((int)overwriteOption == 2)
				{
					throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.PermissionMismatch);
				}
				if ((int)overwriteOption == 3 || (int)overwriteOption == 0)
				{
					if (condition != null && (condition.IfModifiedSinceTime.HasValue || condition.IfLastModificationTimeMatch != null && (int)condition.IfLastModificationTimeMatch.Length > 0))
					{
						throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.PermissionMismatch);
					}
					Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Changing overwrite option to create only");
					overwriteOption = OverwriteOption.CreateNewOnly;
				}
			}
		}

		private static void ApplySourceConditions(HttpWebRequest request, BlobObjectCondition sourceConditions)
		{
			if (sourceConditions != null)
			{
				if (sourceConditions.IfModifiedSinceTime.HasValue)
				{
					request.IfModifiedSince = sourceConditions.IfModifiedSinceTime.Value;
				}
				if (sourceConditions.IfNotModifiedSinceTime.HasValue)
				{
					WebHeaderCollection headers = request.Headers;
					DateTime? ifNotModifiedSinceTime = sourceConditions.IfNotModifiedSinceTime;
					headers["If-Unmodified-Since"] = HttpUtilities.ConvertDateTimeToHttpString(ifNotModifiedSinceTime.Value);
				}
				if (sourceConditions.IfLastModificationTimeMatch != null && (int)sourceConditions.IfLastModificationTimeMatch.Length > 0)
				{
					request.Headers["If-Match"] = ETagHelper.GetETagFromDateTime(sourceConditions.IfLastModificationTimeMatch[0], true);
				}
				if (sourceConditions.IfLastModificationTimeMismatch != null && (int)sourceConditions.IfLastModificationTimeMismatch.Length > 0)
				{
					request.Headers["If-None-Match"] = ETagHelper.GetETagFromDateTime(sourceConditions.IfLastModificationTimeMismatch[0], true);
				}
			}
		}

		[SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification="if-def")]
		private IEnumerator<IAsyncResult> AsynchronousCopyBlobImpl(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, UriString copySource, bool isLargeBlockBlobAllowed, bool is8TBPageBlobAllowed, FECopyType copyType, DateTime? sourceSnapshot, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ICopyBlobResult> context)
		{
			IAsyncResult asyncResult;
			BlobType blobType;
			bool flag;
			long num;
			string str;
			NameValueCollection nameValueCollection;
			byte[] numArray;
			SequenceNumberUpdate sequenceNumberUpdate;
			string str1;
			Duration startingNow = Duration.StartingNow;
			CopyBlobResult elapsed = new CopyBlobResult()
			{
				CopySourceVerificationRequestRoundTripLatency = TimeSpan.Zero
			};
			context.ResultData = elapsed;
			if (accessIdentifier == null)
			{
				throw new ArgumentNullException("accessIdentifier");
			}
			if (string.IsNullOrEmpty(destinationAccount))
			{
				throw new ArgumentException("destinationAccount", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(destinationContainer))
			{
				throw new ArgumentException("destinationContainer", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(destinationBlob))
			{
				throw new ArgumentException("destinationBlob", "Cannot be null or empty");
			}
			if (copySource == null)
			{
				throw new ArgumentNullException("copySource");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in CopyBlob");
			}
			if (!requestContext.IsRequestVersionAtLeastApril15)
			{
				asyncResult = this.authorizationManager.BeginCheckAccess(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, PermissionLevel.Write, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
				yield return asyncResult;
				this.authorizationManager.EndCheckAccess(asyncResult);
			}
			else
			{
				List<SASPermission> sASPermissions = new List<SASPermission>()
				{
					SASPermission.Write,
					SASPermission.Create
				};
				if (this.authorizationManager.CheckAccessWithMultiplePermissions(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, PermissionLevel.Write, SasType.ResourceSas | SasType.AccountSas, SasResourceType.Object, sASPermissions, startingNow.Remaining(timeout)).SignedPermission == SASPermission.Create)
				{
					this.AuthorizationCondition = new Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.AuthorizationCondition()
					{
						MustNotExist = true
					};
					this.ApplyAuthorizationConditions(base.AuthorizationCondition, destinationCondition, ref destinationOverwriteOption);
				}
			}
			Duration duration = Duration.StartingNow;
			string str2 = RealServiceManager.GetCopySourceRequestVersion(requestContext);
			asyncResult = RealServiceManager.BeginGetResponseFromCopySource("HEAD", copySource, str2, sourceCondition, startingNow.Remaining(RealServiceManager.GetMaxTimeoutForCopySourceRequest(timeout)), context.GetResumeCallback(), context.GetResumeState("BeginGetResponseFromCopySource"));
			yield return asyncResult;
			HttpWebResponse httpWebResponse = null;
			try
			{
				httpWebResponse = RealServiceManager.EndGetResponseFromCopySource(asyncResult);
				if (string.IsNullOrEmpty(httpWebResponse.Headers["x-ms-request-id"]) && RealServiceManager.ShouldRetryWithGetRequest(httpWebResponse.StatusCode))
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] statusCode = new object[] { httpWebResponse.StatusCode, null };
					statusCode[1] = duration.Elapsed.TotalMilliseconds;
					verbose.Log("FE CopyBlob: Hit non-Azure blob with {0} HEAD response, closing response and retrying with GET. Request RTT took {1}ms.", statusCode);
					RealServiceManager.CloseResponse(httpWebResponse);
					asyncResult = RealServiceManager.BeginGetResponseFromCopySource("GET", copySource, str2, sourceCondition, startingNow.Remaining(RealServiceManager.GetMaxTimeoutForCopySourceRequest(timeout)), context.GetResumeCallback(), context.GetResumeState("BeginGetResponseFromCopySource"));
					yield return asyncResult;
					httpWebResponse = RealServiceManager.EndGetResponseFromCopySource(asyncResult);
				}
				RealServiceManager.VerifyCopySourceAccess(httpWebResponse);
				RealServiceManager.ExtractAsynchronousCopyBlobArguments(httpWebResponse, destinationAccount, destinationContainer, destinationBlob, (CopyBlobProperties)copyBlobProperties, sourceSnapshot, copyType, isLargeBlockBlobAllowed, is8TBPageBlobAllowed, out blobType, out flag, out num, out str, out nameValueCollection, out numArray, out sequenceNumberUpdate, out str1);
				StorageStampHelpers.ValidateServiceProperties(nameValueCollection, str);
			}
			finally
			{
				elapsed.CopySourceVerificationRequestRoundTripLatency = duration.Elapsed;
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] totalMilliseconds = new object[] { elapsed.CopySourceVerificationRequestRoundTripLatency.TotalMilliseconds };
				stringDataEventStream.Log("FE CopyBlob: copy source verification request round trip latency: {0}ms", totalMilliseconds);
				RealServiceManager.CloseResponse(httpWebResponse);
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(destinationAccount, destinationContainer))
			{
				using (IBlobObject blobObject = blobContainer.CreateBlobObjectInstance(blobType, destinationBlob))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginAsynchronousCopyBlob(copySource, flag, copyType, num, str, nameValueCollection, numArray, sequenceNumberUpdate, destinationOverwriteOption, str1, destinationCondition, context.GetResumeCallback(), context.GetResumeState("destBlobObject.BeginAsynchronousCopyBlob"));
					yield return asyncResult;
					CopyBlobOperationInfo copyBlobOperationInfo = blobObject.EndAsynchronousCopyBlob(asyncResult);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "CopyBlob call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), destinationAccount, destinationContainer, destinationBlob);
					NephosAssertionException.Assert(copyBlobOperationInfo != null, "CopyBlob call for blob {0} has completed successfully, however copy info is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), destinationAccount, destinationContainer, destinationBlob);
					elapsed.LastModifiedTime = blobObject.LastModificationTime.Value;
					elapsed.CopyInfo = copyBlobOperationInfo;
				}
			}
		}

		public override IAsyncResult BeginAbortCopyBlob(IAccountIdentifier identifier, string account, string container, string blob, Guid copyId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.AbortCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.AbortCopyBlobImpl(identifier, account, container, blob, copyId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginAcquireBlobLease(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.AcquireBlobLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireBlobLeaseImpl(identifier, account, container, blob, leaseType, leaseDuration, proposedLeaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginAcquireContainerLease(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, TimeSpan leaseDuration, Guid? proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.AcquireContainerLease", callback, state);
			asyncIteratorContext.Begin(this.AcquireContainerLeaseImpl(identifier, account, container, leaseType, leaseDuration, proposedLeaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginAppendBlock(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool supportCRC64, BlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IAppendBlockResult> asyncIteratorContext = new AsyncIteratorContext<IAppendBlockResult>("RealServiceManager.AppendBlock", callback, state);
			asyncIteratorContext.Begin(this.AppendBlockImpl(identifier, account, container, blob, inputStream, contentLength, contentCRC64, contentMD5, supportCRC64, condition, conditionalMaxBlobSize, conditionalAppendBlockPosition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		[SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification="if-def")]
		public override IAsyncResult BeginAsynchronousCopyBlob(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, UriString copySource, bool isLargeBlockBlobAllowed, bool is8TBPageBlobAllowed, FECopyType copyType, DateTime? sourceSnapshot, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ICopyBlobResult> asyncIteratorContext = new AsyncIteratorContext<ICopyBlobResult>("RealServiceManager.AsynchronousCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.AsynchronousCopyBlobImpl(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, copySource, isLargeBlockBlobAllowed, is8TBPageBlobAllowed, copyType, sourceSnapshot, copyBlobProperties, destinationCondition, destinationOverwriteOption, sourceCondition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginBreakBlobLease(IAccountIdentifier identifier, string account, string container, string blob, TimeSpan? leaseBreakPeriod, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.BreakBlobLease", callback, state);
			asyncIteratorContext.Begin(this.BreakBlobLeaseImpl(identifier, account, container, blob, leaseBreakPeriod, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginBreakContainerLease(IAccountIdentifier identifier, string account, string container, TimeSpan? leaseBreakPeriod, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.BreakContainerLease", callback, state);
			asyncIteratorContext.Begin(this.BreakContainerLeaseImpl(identifier, account, container, leaseBreakPeriod, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginChangeBlobLease(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, Guid proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.ChangeBlobLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeBlobLeaseImpl(identifier, account, container, blob, leaseId, proposedLeaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginChangeContainerLease(IAccountIdentifier identifier, string account, string container, Guid leaseId, Guid proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.ChangeContainerLease", callback, state);
			asyncIteratorContext.Begin(this.ChangeContainerLeaseImpl(identifier, account, container, leaseId, proposedLeaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginClearPage(IAccountIdentifier identifier, string account, string container, string blob, long offset, long length, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IClearPageResult> asyncIteratorContext = new AsyncIteratorContext<IClearPageResult>("RealServiceManager.ClearPage", callback, state);
			asyncIteratorContext.Begin(this.ClearPageImpl(identifier, account, container, blob, new BlobRegion(offset, length), condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginCreateContainer(IAccountIdentifier identifier, string account, string container, NameValueCollection metadata, BaseAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("RealServiceManager.CreateContainer", callback, state);
			asyncIteratorContext.Begin(this.CreateContainerImpl(identifier, account, container, metadata, acl, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginDeleteBlob(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.DeleteBlob", callback, state);
			asyncIteratorContext.Begin(this.DeleteBlobImpl(identifier, account, container, blob, condition, snapshot, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginDeleteContainer(IAccountIdentifier identifier, string account, string container, Guid? leaseId, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, DateTime? snapshotTimestamp, bool? isRequiringNoSnapshots, bool? isDeletingOnlySnapshots, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.DeleteContainer", callback, state);
			asyncIteratorContext.Begin(this.DeleteContainerImpl(identifier, account, container, leaseId, ifModifiedSince, ifNotModifiedSince, snapshotTimestamp, isRequiringNoSnapshots, isDeletingOnlySnapshots, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream outputStream, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobPropertiesCallback, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.GetBlob", callback, state);
			asyncIteratorContext.Begin(this.GetBlobImpl(identifier, account, container, blob, outputStream, null, false, false, additionalPropertyNames, condition, snapshot, timeout, excludeNonSystemHeaders, interceptBlobPropertiesCallback, null, null, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream outputStream, long startOffset, long numberOfBytes, bool isCalculatingCrc64ForRange, bool isCalculatingMD5ForRange, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobPropertiesCallback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeCrc64 interceptRangeCrc64Callback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeMD5 interceptRangeMD5Callback, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.GetBlob", callback, state);
			asyncIteratorContext.Begin(this.GetBlobImpl(identifier, account, container, blob, outputStream, new BlobRegion(startOffset, numberOfBytes), isCalculatingCrc64ForRange, isCalculatingMD5ForRange, additionalPropertyNames, condition, snapshot, timeout, excludeNonSystemHeaders, interceptBlobPropertiesCallback, interceptRangeCrc64Callback, interceptRangeMD5Callback, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlobMetadata(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AsyncHelper.Tuple<DateTime, NameValueCollection>> asyncIteratorContext = new AsyncIteratorContext<AsyncHelper.Tuple<DateTime, NameValueCollection>>("RealServiceManager.GetBlobMetadata", callback, state);
			asyncIteratorContext.Begin(this.GetBlobMetadataImpl(identifier, account, container, blob, condition, snapshot, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlobProperties(IAccountIdentifier identifier, string account, string container, string blob, bool supportCrc64, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlobProperties> asyncIteratorContext = new AsyncIteratorContext<IBlobProperties>("RealServiceManager.GetBlobProperties", callback, state);
			asyncIteratorContext.Begin(this.GetBlobPropertiesImpl(identifier, account, container, blob, supportCrc64, condition, snapshot, timeout, excludeNonSystemHeaders, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlobServiceProperties(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = new AsyncIteratorContext<AnalyticsSettings>("RealServiceManager.GetBlobServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.GetBlobServicePropertiesImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlobServiceStats(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<GeoReplicationStats> asyncIteratorContext = new AsyncIteratorContext<GeoReplicationStats>("RealServiceManager.GetBlobServiceStats", callback, state);
			asyncIteratorContext.Begin(this.GetBlobServiceStatsImpl(identifier, ownerAccountName, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetBlockList(IAccountIdentifier identifier, string account, string container, string blob, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = new AsyncIteratorContext<IBlockLists>("RealServiceManager.GetBlockList", callback, state);
			asyncIteratorContext.Begin(this.GetBlockListImpl(identifier, account, container, blob, blockListTypes, blobServiceVersion, condition, snapshot, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetContainerAcl(IAccountIdentifier identifier, string account, string container, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<AsyncHelper.Tuple<DateTime, ContainerAclSettings>> asyncIteratorContext = new AsyncIteratorContext<AsyncHelper.Tuple<DateTime, ContainerAclSettings>>("RealServiceManager.GetContainerAcl", callback, state);
			asyncIteratorContext.Begin(this.GetContainerAclImpl(identifier, account, container, leaseId, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetContainerMetadata(IAccountIdentifier identifier, string account, string container, DateTime? snapshot, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IContainerProperties> asyncIteratorContext = new AsyncIteratorContext<IContainerProperties>("RealServiceManager.GetContainerMetadata", callback, state);
			asyncIteratorContext.Begin(this.GetContainerMetadataImpl(identifier, account, container, snapshot, ifModifiedSince, leaseId, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetContainerProperties(IAccountIdentifier identifier, string account, string container, DateTime? snapshot, DateTime? ifModifiedSinceTime, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IContainerProperties> asyncIteratorContext = new AsyncIteratorContext<IContainerProperties>("RealServiceManager.GetContainerProperties", callback, state);
			asyncIteratorContext.Begin(this.GetContainerPropertiesImpl(identifier, account, container, snapshot, ifModifiedSinceTime, leaseId, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginGetPageRangeList(IAccountIdentifier identifier, string account, string container, string blob, long offset, long length, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IGetPageRangeListResult> asyncIteratorContext = new AsyncIteratorContext<IGetPageRangeListResult>("RealServiceManager.GetPageRangeList", callback, state);
			asyncIteratorContext.Begin(this.GetPageRangeListImpl(identifier, account, container, blob, new BlobRegion(offset, length), additionalPropertyNames, condition, snapshot, maxPageRanges, prevSnapshotTimestamp, isRangeCompressed, skipClearPages, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private static IAsyncResult BeginGetResponseFromCopySource(string requestMethod, UriString copySource, string requestVersion, BlobObjectCondition sourceCondition, TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<HttpWebResponse> asyncIteratorContext = new AsyncIteratorContext<HttpWebResponse>("ServiceManager.BeginGetResponseFromCopySource", callback, state);
			asyncIteratorContext.Begin(RealServiceManager.GetResponseFromCopySourceImpl(requestMethod, copySource, requestVersion, sourceCondition, timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginListBlobs(IAccountIdentifier identifier, string account, string container, BlobServiceVersion version, string blobPrefix, string delimiter, string containerMarker, string blobMarker, DateTime? snapshotMarker, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, bool includeDisabledContainers, bool isFetchingMetadata, bool isIncludingSnapshots, bool isIncludingPageBlobs, bool isIncludingAppendBlobs, bool isIncludingUncommittedBlobs, bool isIncludingLeaseStatus, int? maxBlobNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IListBlobsResultCollection> asyncIteratorContext = new AsyncIteratorContext<IListBlobsResultCollection>("RealServiceManager.ListBlobs", callback, state);
			asyncIteratorContext.Begin(this.ListBlobsImpl(identifier, account, container, version, blobPrefix, delimiter, containerMarker, blobMarker, snapshotMarker, ifModifiedSinceTime, ifNotModifiedSinceTime, includeDisabledContainers, isFetchingMetadata, isIncludingSnapshots, isIncludingPageBlobs, isIncludingAppendBlobs, isIncludingUncommittedBlobs, isIncludingLeaseStatus, maxBlobNames, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginListContainers(IAccountIdentifier identifier, string account, string containerPrefix, string delimiter, string marker, bool includeDisabledContainers, bool includeSnapshots, bool isFetchingLeaseInfo, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, int? maxContainerNames, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IListContainersResultCollection> asyncIteratorContext = new AsyncIteratorContext<IListContainersResultCollection>("RealServiceManager.ListContainers", callback, state);
			asyncIteratorContext.Begin(this.ListContainersImpl(identifier, account, containerPrefix, delimiter, marker, includeDisabledContainers, includeSnapshots, isFetchingLeaseInfo, ifModifiedSinceTime, ifNotModifiedSinceTime, maxContainerNames, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutBlob(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, IPutBlobProperties putBlobProperties, bool supportCrc64, bool calculateCrc64, bool storeCrc64, bool verifyCrc64, bool calculateMd5, bool storeMd5, bool verifyMd5, bool generationIdEnabled, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, BlobObjectCondition condition, OverwriteOption overwriteOption, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPutBlobResult> asyncIteratorContext = new AsyncIteratorContext<IPutBlobResult>("RealServiceManager.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(identifier, account, container, blob, inputStream, contentLength, putBlobProperties, supportCrc64, calculateCrc64, storeCrc64, verifyCrc64, calculateMd5, storeMd5, verifyMd5, generationIdEnabled, isLargeBlockBlobRequest, is8TBPageBlobAllowed, condition, overwriteOption, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutBlobFromBlocks(IAccountIdentifier identifier, string account, string container, string blob, BlobServiceVersion blobServiceVersion, byte[][] blockIdList, BlockSource[] blockSourceList, IPutBlobProperties putBlobProperties, BlobObjectCondition condition, OverwriteOption overwriteOption, IUpdateOptions updateOptions, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPutBlobFromBlocksResult> asyncIteratorContext = new AsyncIteratorContext<IPutBlobFromBlocksResult>("RealServiceManager.PutBlobFromBlocks", callback, state);
			asyncIteratorContext.Begin(this.PutBlobFromBlocksImpl(identifier, account, container, blob, blobServiceVersion, blockIdList, blockSourceList, putBlobProperties, condition, overwriteOption, updateOptions, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutBlock(IAccountIdentifier identifier, string account, string container, string blob, byte[] blockIdentifier, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool isLargeBlockBlobRequest, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPutBlockResult> asyncIteratorContext = new AsyncIteratorContext<IPutBlockResult>("RealServiceManager.PutBlock", callback, state);
			asyncIteratorContext.Begin(this.PutBlockImpl(identifier, account, container, blob, blockIdentifier, inputStream, contentLength, contentCRC64, contentMD5, isLargeBlockBlobRequest, supportCRC64, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutPage(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long offset, long length, long? contentCRC64, byte[] contentMD5, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPutPageResult> asyncIteratorContext = new AsyncIteratorContext<IPutPageResult>("RealServiceManager.PutPage", callback, state);
			asyncIteratorContext.Begin(this.PutPageImpl(identifier, account, container, blob, inputStream, new BlobRegion(offset, length), contentCRC64, contentMD5, supportCRC64, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginReleaseBlobLease(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.ReleaseBlobLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseBlobLeaseImpl(identifier, account, container, blob, leaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginReleaseContainerLease(IAccountIdentifier identifier, string account, string container, Guid leaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.ReleaseContainerLease", callback, state);
			asyncIteratorContext.Begin(this.ReleaseContainerLeaseImpl(identifier, account, container, leaseId, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginRenewBlobLease(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.RenewBlobLease", callback, state);
			asyncIteratorContext.Begin(this.RenewBlobLeaseImpl(identifier, account, container, blob, leaseType, leaseId, leaseDuration, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginRenewContainerLease(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ILeaseInfoResult> asyncIteratorContext = new AsyncIteratorContext<ILeaseInfoResult>("RealServiceManager.RenewContainerLease", callback, state);
			asyncIteratorContext.Begin(this.RenewContainerLeaseImpl(identifier, account, container, leaseType, leaseId, leaseDuration, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSetBlobMetadata(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ISetBlobMetadataResult> asyncIteratorContext = new AsyncIteratorContext<ISetBlobMetadataResult>("RealServiceManager.SetBlobMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetBlobMetadataImpl(identifier, account, container, blob, metadata, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSetBlobProperties(IAccountIdentifier identifier, string account, string container, string blob, IPutBlobProperties properties, bool supportCrc64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ISetBlobPropertiesResult> asyncIteratorContext = new AsyncIteratorContext<ISetBlobPropertiesResult>("RealServiceManager.SetBlobProperties", callback, state);
			asyncIteratorContext.Begin(this.SetBlobPropertiesImpl(identifier, account, container, blob, properties, supportCrc64, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSetBlobServiceProperties(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("RealServiceManager.SetBlobServiceProperties", callback, state);
			asyncIteratorContext.Begin(this.SetBlobServicePropertiesImpl(identifier, ownerAccountName, settings, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSetContainerAcl(IAccountIdentifier identifier, string account, string container, ContainerAclSettings acl, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("RealServiceManager.SetContainerAcl", callback, state);
			asyncIteratorContext.Begin(this.SetContainerAclImpl(identifier, account, container, acl, ifModifiedSince, ifNotModifiedSince, leaseId, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSetContainerMetadata(IAccountIdentifier identifier, string account, string container, NameValueCollection metadata, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("RealServiceManager.SetContainerMetadata", callback, state);
			asyncIteratorContext.Begin(this.SetContainerMetadataImpl(identifier, account, container, metadata, ifModifiedSince, leaseId, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSnapshotBlob(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ISnapshotBlobResult> asyncIteratorContext = new AsyncIteratorContext<ISnapshotBlobResult>("RealServiceManager.SnapshotBlob", callback, state);
			asyncIteratorContext.Begin(this.SnapshotBlobImpl(identifier, account, container, blob, metadata, condition, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		[SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification="if-def")]
		public override IAsyncResult BeginSynchronousCopyBlob(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, string sourceAccount, string sourceContainer, string sourceBlob, UriString copySource, bool isSourceSignedAccess, bool isDestinationSignedAccess, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, DateTime? sourceSnapshot, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<ICopyBlobResult> asyncIteratorContext = new AsyncIteratorContext<ICopyBlobResult>("RealServiceManager.SynchronousCopyBlob", callback, state);
			asyncIteratorContext.Begin(this.SynchronousCopyBlobImpl(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, sourceAccount, sourceContainer, sourceBlob, copySource, isSourceSignedAccess, isDestinationSignedAccess, copyBlobProperties, destinationCondition, destinationOverwriteOption, sourceCondition, sourceSnapshot, timeout, requestContext, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private IEnumerator<IAsyncResult> BreakBlobLeaseImpl(IAccountIdentifier identifier, string account, string container, string blob, TimeSpan? leaseBreakPeriod, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in BreakBlobLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.BreakBlobLeaseImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			BlobServiceVersion blobServiceVersion = BlobServiceVersion.Original;
			if (requestContext.IsRequestVersionAtLeastFebruary12)
			{
				blobServiceVersion = BlobServiceVersion.Feb12;
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob, DateTime.MaxValue, blobServiceVersion))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginBreakLease(leaseBreakPeriod, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.BreakBlobLeaseImpl"));
					yield return asyncResult;
					blobObject.EndBreakLease(asyncResult);
					ILeaseInfo leaseInfo = blobObject.LeaseInfo;
					NephosAssertionException.Assert(leaseInfo != null, "BreakBlobLease call for blob {0} completed successfully, but the lease info is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					Guid? id = leaseInfo.Id;
					NephosAssertionException.Assert(id.HasValue, "BreakBlobLease call for blob {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					TimeSpan? nullable = leaseInfo.Duration;
					NephosAssertionException.Assert(nullable.HasValue, "BreakBlobLease call for blob {0} completed successfully, but the lease duration is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "BreakBlobLease call for blob {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value,
						LeaseInfo = leaseInfo
					};
					context.ResultData = leaseInfoResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> BreakContainerLeaseImpl(IAccountIdentifier identifier, string account, string container, TimeSpan? leaseBreakPeriod, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in BreakLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginBreakLease(leaseBreakPeriod, condition, !requestContext.IsRequestVersionAtLeastAugust13, requestContext.IsRequestVersionAtLeastMay16, context.GetResumeCallback(), context.GetResumeState("IBlobContainer.BeginBreakLease"));
				yield return asyncResult;
				blobContainer.EndBreakLease(asyncResult);
				ILeaseInfo leaseInfo = blobContainer.LeaseInfo;
				NephosAssertionException.Assert(leaseInfo != null, "BreakContainerLease call for container {0} completed successfully, but the lease info is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				Guid? id = leaseInfo.Id;
				NephosAssertionException.Assert(id.HasValue, "BreakContainerLease call for container {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				TimeSpan? nullable = leaseInfo.Duration;
				NephosAssertionException.Assert(nullable.HasValue, "BreakContainerLease call for container {0} completed successfully, but the lease duration is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "BreakContainerLease call for container {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
				{
					LastModifiedTime = blobContainer.LastModificationTime.Value,
					LeaseInfo = leaseInfo
				};
				context.ResultData = leaseInfoResult;
			}
		}

		private IEnumerator<IAsyncResult> ChangeBlobLeaseImpl(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, Guid proposedLeaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ChangeBlobLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ChangeBlobLeaseImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginChangeLease(leaseId, proposedLeaseId, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ChangeBlobLeaseImpl"));
					yield return asyncResult;
					blobObject.EndChangeLease(asyncResult);
					ILeaseInfo leaseInfo = blobObject.LeaseInfo;
					NephosAssertionException.Assert(leaseInfo != null, "ChangeBlobLease call for blob {0} completed successfully, but the lease info is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					Guid? id = leaseInfo.Id;
					NephosAssertionException.Assert(id.HasValue, "ChangeBlobLease call for blob {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "ChangeBlobLease call for blob {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value,
						LeaseInfo = leaseInfo
					};
					context.ResultData = leaseInfoResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> ChangeContainerLeaseImpl(IAccountIdentifier identifier, string account, string container, Guid leaseId, Guid proposedLeaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ChangeContainerLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginChangeLease(leaseId, proposedLeaseId, condition, !requestContext.IsRequestVersionAtLeastAugust13, requestContext.IsRequestVersionAtLeastMay16, context.GetResumeCallback(), context.GetResumeState("IBlobContainer.BeginChangeLease"));
				yield return asyncResult;
				blobContainer.EndChangeLease(asyncResult);
				ILeaseInfo leaseInfo = blobContainer.LeaseInfo;
				NephosAssertionException.Assert(leaseInfo != null, "ChangeContainerLease call for container {0} completed successfully, but the lease info is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				Guid? id = leaseInfo.Id;
				NephosAssertionException.Assert(id.HasValue, "ChangeContainerLease call for container {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "ChangeContainerLease call for container {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
				{
					LastModifiedTime = blobContainer.LastModificationTime.Value,
					LeaseInfo = leaseInfo
				};
				context.ResultData = leaseInfoResult;
			}
		}

		private IEnumerator<IAsyncResult> ClearPageImpl(IAccountIdentifier identifier, string account, string container, string blob, BlobRegion blobRegion, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IClearPageResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (blobRegion == null)
			{
				throw new ArgumentNullException("blobRegion");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ClearPage");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ClearPageImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IIndexBlobObject indexBlobObject = blobContainer.CreateIndexBlobObjectInstance(blob))
				{
					indexBlobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = indexBlobObject.BeginClearPage(blobRegion, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ClearPageImpl"));
					yield return asyncResult;
					indexBlobObject.EndClearPage(asyncResult);
					ClearPageResult value = new ClearPageResult();
					DateTime? lastModificationTime = indexBlobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "ClearPage call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					value.LastModifiedTime = indexBlobObject.LastModificationTime.Value;
					long? sequenceNumber = indexBlobObject.SequenceNumber;
					NephosAssertionException.Assert(sequenceNumber.HasValue, "ClearPage call for blob {0} has completed successfully, however the sequence number is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					value.SequenceNumber = indexBlobObject.SequenceNumber.Value;
					context.ResultData = value;
				}
			}
		}

		private static void CloseResponse(HttpWebResponse response)
		{
			if (response != null)
			{
				try
				{
					response.Close();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] str = new object[] { exception.GetType().ToString(), exception.Message };
					verbose.Log("FE CopyBlob: Hit exception while closing the HTTP response to copy source: {0}: {1}", str);
				}
			}
		}

		private void ComputeAndLogCrc(byte[] data, int offset, int count, long totalBytesReadSoFar)
		{
			ulong num = (ulong)0;
			if (count > 0)
			{
				num = (ulong)CrcUtils.ComputeCrc(data, offset, count);
			}
			IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] objArray = new object[] { count, totalBytesReadSoFar, num };
			info.Log("Calculating CRC of Data. Block bytes: {0}, total bytes: {1}, CRC: 0x{2:x}", objArray);
		}

		private static void ConvertCopyBlobProperties(string resourceAccount, string resourceContainer, string resourceIdentifier, ICopyBlobProperties properties, out byte[] applicationMetadata)
		{
			applicationMetadata = null;
			if (properties.BlobMetadata != null)
			{
				try
				{
					applicationMetadata = MetadataEncoding.Encode(properties.BlobMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] resourceString = new object[] { RealServiceManager.GetResourceString(resourceAccount, resourceContainer, resourceIdentifier), metadataFormatException.Message };
					throw new MetadataFormatException(string.Format(invariantCulture, "Invalid metadata for blob {0}: {1}", resourceString), metadataFormatException);
				}
			}
		}

		private void ConvertPutBlobProperties(string resource, IPutBlobProperties properties, out byte[] applicationMetadata, out string contentType, out long? contentCrc64, out byte[] contentMD5)
		{
			applicationMetadata = null;
			if (properties.BlobMetadata != null)
			{
				try
				{
					applicationMetadata = MetadataEncoding.Encode(properties.BlobMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { resource, metadataFormatException.Message };
					throw new MetadataFormatException(string.Format(invariantCulture, "Invalid metadata for blob {0}: {1}", objArray), metadataFormatException);
				}
			}
			contentType = properties.ContentType;
			contentCrc64 = properties.ContentCrc64;
			contentMD5 = properties.ContentMD5;
		}

		private void ConvertSetBlobProperties(string resource, IPutBlobProperties properties, bool supportCrc64, out NameValueCollection serviceMetadata, out string contentType, out long? maxBlobSize)
		{
			string str;
			maxBlobSize = properties.MaxBlobSize;
			bool flag = (!string.IsNullOrEmpty(properties.ContentType) || !string.IsNullOrEmpty(properties.ContentEncoding) || !string.IsNullOrEmpty(properties.ContentLanguage) || !string.IsNullOrEmpty(properties.CacheControl) || !string.IsNullOrEmpty(properties.ContentDisposition) || properties.ContentMD5 != null ? false : !properties.ContentCrc64.HasValue);
			if (flag && (maxBlobSize.HasValue ? true : properties.SequenceNumberUpdate != null))
			{
				serviceMetadata = null;
				contentType = null;
				return;
			}
			serviceMetadata = new NameValueCollection();
			if (properties.ContentType != null)
			{
				contentType = properties.ContentType;
			}
			else
			{
				contentType = string.Empty;
			}
			RealServiceManager.AddServiceProperty(resource, serviceMetadata, RealServiceManager.ContentEncodingTag, properties.ContentEncoding);
			RealServiceManager.AddServiceProperty(resource, serviceMetadata, RealServiceManager.ContentLanguageTag, properties.ContentLanguage);
			RealServiceManager.AddServiceProperty(resource, serviceMetadata, RealServiceManager.CacheControlTag, properties.CacheControl);
			RealServiceManager.AddServiceProperty(resource, serviceMetadata, RealServiceManager.ContentDispositionTag, properties.ContentDisposition);
			if (supportCrc64)
			{
				RealServiceManager.SetServiceProperty(resource, serviceMetadata, RealServiceManager.ContentCrc64Tag, properties.ContentCrc64.ToBase64String());
			}
			str = (properties.ContentMD5 == null ? RealServiceManager.EmptyContentMD5Value : Convert.ToBase64String(properties.ContentMD5));
			RealServiceManager.AddServiceProperty(resource, serviceMetadata, RealServiceManager.ContentMD5Tag, str);
		}

		private IEnumerator<IAsyncResult> CreateContainerImpl(IAccountIdentifier identifier, string accountName, string containerName, NameValueCollection metadata, BaseAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<DateTime> context)
		{
			byte[] numArray;
			DateTime? nullable;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(containerName))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (acl == null)
			{
				throw new ArgumentNullException("acl");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in CreateContainer");
			}
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Create
			};
			this.authorizationManager.CheckAccessWithMultiplePermissions(identifier, accountName, null, null, PermissionLevel.Write, SasType.AccountSas, SasResourceType.Container, sASPermissions, startingNow.Remaining(timeout));
			byte[] numArray1 = null;
			if (metadata != null)
			{
				try
				{
					numArray1 = MetadataEncoding.Encode(metadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					throw new MetadataFormatException(string.Format("Failed to encode metadata for container {0}: {1}", RealServiceManager.GetResourceString(accountName, containerName), metadataFormatException.Message), metadataFormatException);
				}
			}
			bool flag = true;
			using (IBaseBlobContainer operationStatus = (IBaseBlobContainer)this.storageManager.CreateContainerInstance(accountName, containerName, requestContext.ServiceType))
			{
				if (requestContext != null)
				{
					operationStatus.OperationStatus = requestContext.OperationStatus;
				}
				ContainerPropertyNames containerPropertyName = ContainerPropertyNames.ApplicationMetadata;
				operationStatus.Timeout = startingNow.Remaining(timeout);
				IAsyncResult asyncResult = operationStatus.BeginGetProperties(containerPropertyName, null, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.CreateContainerImpl"));
				yield return asyncResult;
				try
				{
					operationStatus.EndGetProperties(asyncResult);
				}
				catch (ContainerNotFoundException containerNotFoundException)
				{
					flag = false;
				}
				if (flag)
				{
					throw new ContainerAlreadyExistsException(string.Concat("Container '", containerName, "' already exists"));
				}
				acl.EncodeToServiceMetadata(out numArray);
				IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
				info.Log("Contacting XAC Server in order to create container '{0}'", new object[] { containerName });
				operationStatus.Timeout = startingNow.Remaining(timeout);
				DateTime? nullable1 = null;
				asyncResult = operationStatus.BeginCreateContainer(nullable1, numArray, numArray1, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.CreateContainerImpl"));
				yield return asyncResult;
				operationStatus.EndCreateContainer(asyncResult);
				nullable = operationStatus.LastModificationTime;
			}
			NephosAssertionException.Assert(nullable.HasValue, "Container {0} has been created successfully, however the last modified time has not been set.", new Func<string, string, string>(RealServiceManager.GetResourceString), accountName, containerName);
			context.ResultData = nullable.Value;
		}

		public static Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager CreateServiceManager(AuthorizationManager authorizationManager, IStorageManager storageManager)
		{
			return new RealServiceManager(authorizationManager, storageManager, new ServiceManagerConfiguration());
		}

		public static Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager CreateServiceManager(AuthorizationManager authorizationManager, IStorageManager storageManager, ServiceManagerConfiguration config)
		{
			return new RealServiceManager(authorizationManager, storageManager, config);
		}

		private IEnumerator<IAsyncResult> DeleteBlobImpl(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			DateTime dateTime;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in DeleteBlob");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Delete
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Delete, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.DeleteBlobImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IBlobObject blobObject = blobContainer1.CreateBaseBlobObjectInstance(str, dateTime))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginSetExpiryTime(DateTime.UtcNow - TimeSpan.FromDays(14), condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.DeleteBlobImpl"));
					yield return asyncResult;
					blobObject.EndSetExpiryTime(asyncResult);
				}
			}
		}

		private IEnumerator<IAsyncResult> DeleteContainerImpl(IAccountIdentifier identifier, string accountName, string containerName, Guid? leaseId, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, DateTime? snapshotTimestamp, bool? isRequiringNoSnapshots, bool? isDeletingOnlySnapshots, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(containerName))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetContainerExpiryTime");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Delete
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, containerName, null, PermissionLevel.Delete, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.DeleteContainer"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			ContainerCondition containerCondition = null;
			if (ifModifiedSince.HasValue || ifNotModifiedSince.HasValue || isRequiringNoSnapshots.HasValue || isDeletingOnlySnapshots.HasValue || snapshotTimestamp.HasValue)
			{
				containerCondition = new ContainerCondition()
				{
					IfModifiedSinceTime = ifModifiedSince,
					IfNotModifiedSinceTime = ifNotModifiedSince,
					SnapshotTimestamp = snapshotTimestamp,
					IsRequiringNoSnapshots = isRequiringNoSnapshots,
					IsDeletingOnlySnapshots = isDeletingOnlySnapshots
				};
			}
			using (IBaseBlobContainer operationStatus = (IBaseBlobContainer)this.storageManager.CreateContainerInstance(accountName, containerName, requestContext.ServiceType))
			{
				if (requestContext != null)
				{
					operationStatus.OperationStatus = requestContext.OperationStatus;
				}
				operationStatus.Timeout = startingNow.Remaining(timeout);
				asyncResult = operationStatus.BeginDeleteContainer(containerCondition, leaseId, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.DeleteContainerImpl"));
				yield return asyncResult;
				operationStatus.EndDeleteContainer(asyncResult);
			}
		}

		private byte[] EncodeServiceMetadata(string resource, NameValueCollection serviceMetadata)
		{
			byte[] numArray;
			if (serviceMetadata == null)
			{
				return null;
			}
			try
			{
				numArray = MetadataEncoding.Encode(serviceMetadata);
			}
			catch (MetadataFormatException metadataFormatException1)
			{
				MetadataFormatException metadataFormatException = metadataFormatException1;
				object[] objArray = new object[] { resource, metadataFormatException.Message };
				NephosAssertionException.Fail(metadataFormatException, "Failed to encode service metadata for blob {0}: {1}", objArray);
				numArray = null;
			}
			return numArray;
		}

		public override void EndAbortCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override ILeaseInfoResult EndAcquireBlobLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndAcquireContainerLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override IAppendBlockResult EndAppendBlock(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IAppendBlockResult> asyncIteratorContext = (AsyncIteratorContext<IAppendBlockResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ICopyBlobResult EndAsynchronousCopyBlob(IAsyncResult asyncResult, out TimeSpan copySourceVerificationRequestRoundTripLatency)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<ICopyBlobResult> asyncIteratorContext = (AsyncIteratorContext<ICopyBlobResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			copySourceVerificationRequestRoundTripLatency = asyncIteratorContext.ResultData.CopySourceVerificationRequestRoundTripLatency;
			if (exception != null)
			{
				if ((exception is BlobAlreadyExistsException || exception is LeaseHeldException) && base.AuthorizationCondition != null && base.AuthorizationCondition.MustNotExist)
				{
					throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.UnauthorizedBlobOverwrite);
				}
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ILeaseInfoResult EndBreakBlobLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndBreakContainerLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndChangeBlobLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndChangeContainerLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override IClearPageResult EndClearPage(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IClearPageResult> asyncIteratorContext = (AsyncIteratorContext<IClearPageResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override DateTime EndCreateContainer(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<DateTime> asyncIteratorContext = (AsyncIteratorContext<DateTime>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override void EndDeleteBlob(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override void EndDeleteContainer(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override void EndGetBlob(IAsyncResult result)
		{
			Exception exception;
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			((AsyncIteratorContext<NoResults>)result).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override DateTime EndGetBlobMetadata(IAsyncResult ar, out NameValueCollection metadata)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<AsyncHelper.Tuple<DateTime, NameValueCollection>> asyncIteratorContext = (AsyncIteratorContext<AsyncHelper.Tuple<DateTime, NameValueCollection>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			metadata = asyncIteratorContext.ResultData.Second;
			return asyncIteratorContext.ResultData.First;
		}

		public override IBlobProperties EndGetBlobProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IBlobProperties> asyncIteratorContext = (AsyncIteratorContext<IBlobProperties>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override AnalyticsSettings EndGetBlobServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<AnalyticsSettings> asyncIteratorContext = (AsyncIteratorContext<AnalyticsSettings>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override GeoReplicationStats EndGetBlobServiceStats(IAsyncResult ar)
		{
			return ar.End<GeoReplicationStats>(RethrowableWrapperBehavior.NoWrap);
		}

		public override IBlockLists EndGetBlockList(IAsyncResult result)
		{
			Exception exception;
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = (AsyncIteratorContext<IBlockLists>)result;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override DateTime EndGetContainerAcl(IAsyncResult ar, out ContainerAclSettings acl)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<AsyncHelper.Tuple<DateTime, ContainerAclSettings>> asyncIteratorContext = (AsyncIteratorContext<AsyncHelper.Tuple<DateTime, ContainerAclSettings>>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			AsyncHelper.Tuple<DateTime, ContainerAclSettings> resultData = asyncIteratorContext.ResultData;
			acl = resultData.Second;
			return resultData.First;
		}

		public override IContainerProperties EndGetContainerMetadata(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IContainerProperties> asyncIteratorContext = (AsyncIteratorContext<IContainerProperties>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IContainerProperties EndGetContainerProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IContainerProperties> asyncIteratorContext = (AsyncIteratorContext<IContainerProperties>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IGetPageRangeListResult EndGetPageRangeList(IAsyncResult result)
		{
			Exception exception;
			if (result == null)
			{
				throw new ArgumentNullException("result");
			}
			AsyncIteratorContext<IGetPageRangeListResult> asyncIteratorContext = (AsyncIteratorContext<IGetPageRangeListResult>)result;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		private static HttpWebResponse EndGetResponseFromCopySource(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<HttpWebResponse> asyncIteratorContext = (AsyncIteratorContext<HttpWebResponse>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IListBlobsResultCollection EndListBlobs(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IListBlobsResultCollection> asyncIteratorContext = (AsyncIteratorContext<IListBlobsResultCollection>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IListContainersResultCollection EndListContainers(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<IListContainersResultCollection> asyncIteratorContext = (AsyncIteratorContext<IListContainersResultCollection>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IPutBlobResult EndPutBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IPutBlobResult> asyncIteratorContext = (AsyncIteratorContext<IPutBlobResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				if ((exception is BlobAlreadyExistsException || exception is LeaseHeldException) && base.AuthorizationCondition != null && base.AuthorizationCondition.MustNotExist)
				{
					throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.UnauthorizedBlobOverwrite);
				}
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IPutBlobFromBlocksResult EndPutBlobFromBlocks(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IPutBlobFromBlocksResult> asyncIteratorContext = (AsyncIteratorContext<IPutBlobFromBlocksResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				if ((exception is BlobAlreadyExistsException || exception is LeaseHeldException) && base.AuthorizationCondition != null && base.AuthorizationCondition.MustNotExist)
				{
					throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.UnauthorizedBlobOverwrite);
				}
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IPutBlockResult EndPutBlock(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IPutBlockResult> asyncIteratorContext = (AsyncIteratorContext<IPutBlockResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override IPutPageResult EndPutPage(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<IPutPageResult> asyncIteratorContext = (AsyncIteratorContext<IPutPageResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ILeaseInfoResult EndReleaseBlobLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndReleaseContainerLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndRenewBlobLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ILeaseInfoResult EndRenewContainerLease(IAsyncResult asyncResult)
		{
			return asyncResult.End<ILeaseInfoResult>(RethrowableWrapperBehavior.NoWrap);
		}

		public override ISetBlobMetadataResult EndSetBlobMetadata(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<ISetBlobMetadataResult> asyncIteratorContext = (AsyncIteratorContext<ISetBlobMetadataResult>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ISetBlobPropertiesResult EndSetBlobProperties(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<ISetBlobPropertiesResult> asyncIteratorContext = (AsyncIteratorContext<ISetBlobPropertiesResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override void EndSetBlobServiceProperties(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			((AsyncIteratorContext<NoResults>)ar).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override DateTime EndSetContainerAcl(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<DateTime> asyncIteratorContext = (AsyncIteratorContext<DateTime>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override DateTime EndSetContainerMetadata(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<DateTime> asyncIteratorContext = (AsyncIteratorContext<DateTime>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ISnapshotBlobResult EndSnapshotBlob(IAsyncResult ar)
		{
			Exception exception;
			if (ar == null)
			{
				throw new ArgumentNullException("ar");
			}
			AsyncIteratorContext<ISnapshotBlobResult> asyncIteratorContext = (AsyncIteratorContext<ISnapshotBlobResult>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override ICopyBlobResult EndSynchronousCopyBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncIteratorContext<ICopyBlobResult> asyncIteratorContext = (AsyncIteratorContext<ICopyBlobResult>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				if ((exception is BlobAlreadyExistsException || exception is LeaseHeldException) && base.AuthorizationCondition != null && base.AuthorizationCondition.MustNotExist)
				{
					throw new NephosUnauthorizedAccessException("User doesn't have sufficient permissions", AuthorizationFailureReason.UnauthorizedBlobOverwrite);
				}
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		[SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification="if-def")]
		private static void ExtractAsynchronousCopyBlobArguments(HttpWebResponse response, string destinationResourceAccount, string destinationResourceContainer, string destinationResourceIdentifier, CopyBlobProperties copyBlobProperties, DateTime? sourceSnapshot, FECopyType copyType, bool isLargeBlockBlobAllowed, bool is8TBPageBlobAllowed, out BlobType blobType, out bool isSourceAzureBlob, out long contentLength, out string contentType, out NameValueCollection serviceMetadataCollection, out byte[] applicationMetadata, out SequenceNumberUpdate sequenceNumberUpdate, out string sourceETag)
		{
			string str;
			blobType = BlobType.ListBlob;
			isSourceAzureBlob = false;
			contentLength = response.ContentLength;
			string item = response.Headers["x-ms-blob-type"];
			if (!string.IsNullOrEmpty(item))
			{
				isSourceAzureBlob = true;
				blobType = BlobTypeStrings.GetBlobType(item);
				long num = (is8TBPageBlobAllowed ? 8796093022208L : 1099511627776L);
				if ((int)blobType == 2 && contentLength > num)
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.RequestEntityTooLarge, "The source request body is too large and exceeds the maximum permissible limit.");
				}
				if ((int)blobType == 0)
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, string.Format("Copy source header {0} value {1} is invalid.", "x-ms-blob-type", item));
				}
			}
			if (copyType == FECopyType.Incremental && (!isSourceAzureBlob || (int)blobType != 2))
			{
				Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Source blob must be page blob for incremental copy operations");
				throw new InvalidSourceBlobTypeException();
			}
			if (!string.IsNullOrEmpty(response.Headers["x-ms-incremental-copy"]) && bool.Parse(response.Headers["x-ms-incremental-copy"]) && !sourceSnapshot.HasValue && string.IsNullOrEmpty(HttpUtilities.GetQueryParameters(response.ResponseUri)["snapshot"]))
			{
				throw new CopySourceCannotBeIncrementalCopyBlobException();
			}
			if (!isSourceAzureBlob)
			{
				if (isLargeBlockBlobAllowed)
				{
					if (response.ContentLength > 5242880000000L)
					{
						throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, "Size of source is greater then maximum allowable size.");
					}
				}
				else if (response.ContentLength > 209715200000L)
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, "Size of source is greater then maximum allowable size.");
				}
			}
			contentType = response.Headers["Content-Type"] ?? string.Empty;
			serviceMetadataCollection = new NameValueCollection();
			string resourceString = RealServiceManager.GetResourceString(destinationResourceAccount, destinationResourceContainer, destinationResourceIdentifier);
			RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.ContentEncodingTag, response.Headers["Content-Encoding"]);
			RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.ContentLanguageTag, response.Headers["Content-Language"]);
			RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.CacheControlTag, response.Headers["Cache-Control"]);
			if (!string.IsNullOrEmpty(response.Headers["x-ms-content-crc64"]))
			{
				RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.ContentCrc64Tag, response.Headers["x-ms-content-crc64"]);
			}
			str = (string.IsNullOrEmpty(response.Headers["Content-MD5"]) ? RealServiceManager.EmptyContentMD5Value : response.Headers["Content-MD5"]);
			RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.ContentMD5Tag, str);
			RealServiceManager.AddCopySourceServiceMetadata(resourceString, serviceMetadataCollection, RealServiceManager.ContentDispositionTag, response.Headers["Content-Disposition"]);
			bool flag = false;
			if (copyBlobProperties.BlobMetadata == null)
			{
				flag = true;
				copyBlobProperties.BlobMetadata = new NameValueCollection();
				HttpRequestAccessorCommon.GetApplicationMetadataFromHeaders(response.Headers, response.Headers["x-ms-version"], copyBlobProperties.BlobMetadata);
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] count = new object[] { copyBlobProperties.BlobMetadata.Count };
				verbose.Log("FE CopyBlob: GetApplicationMetadataFromSourceHeaders >> Found {0} metadata items.", count);
			}
			try
			{
				RealServiceManager.ConvertCopyBlobProperties(destinationResourceAccount, destinationResourceContainer, destinationResourceIdentifier, copyBlobProperties, out applicationMetadata);
			}
			catch (MetadataFormatException metadataFormatException1)
			{
				MetadataFormatException metadataFormatException = metadataFormatException1;
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("FE CopyBlob: MetadataFormatException while encoding application metadata: {0}", new object[] { metadataFormatException.Message });
				if (flag)
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, "Copy source contains invalid metadata", metadataFormatException);
				}
				throw;
			}
			sequenceNumberUpdate = null;
			long? sequenceNumberFromHeader = RealServiceManager.GetSequenceNumberFromHeader(response.Headers["x-ms-blob-sequence-number"]);
			if (sequenceNumberFromHeader.HasValue)
			{
				if ((int)blobType != 2)
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, string.Format("Copy source header {0} not supported.", "x-ms-blob-sequence-number"));
				}
				sequenceNumberUpdate = new SequenceNumberUpdate(SequenceNumberUpdateType.Update, sequenceNumberFromHeader.Value);
			}
			else if ((int)blobType == 2)
			{
				throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, string.Format("Copy source required header {0} is missing.", "x-ms-blob-sequence-number"));
			}
			sourceETag = response.Headers["ETag"];
		}

		private IBlobProperties ExtractBlobProperties(string resourceAccount, string resourceContainer, string resourceBlob, IBlobObject blobObject, bool supportCrc64, out bool isMD5MigrationRequired)
		{
			bool flag;
			BlobProperties blobProperty = new BlobProperties();
			if (blobObject.ApplicationMetadata != null)
			{
				try
				{
					MetadataEncoding.Decode(blobObject.ApplicationMetadata, blobProperty.BlobMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] resourceString = new object[] { RealServiceManager.GetResourceString(resourceAccount, resourceContainer, resourceBlob) };
					throw new NephosStorageDataCorruptionException(string.Format(invariantCulture, "Error decoding application metadata for blob {0}", resourceString), metadataFormatException);
				}
			}
			NephosAssertionException.Assert(blobObject.ServiceMetadata != null, "Service metadata for blob {0} does not exist, however service metadata is always created for blobs put through Nephos Storage.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), resourceAccount, resourceContainer, resourceBlob);
			NameValueCollection nameValueCollection = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
			try
			{
				MetadataEncoding.Decode(blobObject.ServiceMetadata, nameValueCollection);
			}
			catch (MetadataFormatException metadataFormatException3)
			{
				MetadataFormatException metadataFormatException2 = metadataFormatException3;
				CultureInfo cultureInfo = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { RealServiceManager.GetResourceString(resourceAccount, resourceContainer, resourceBlob) };
				throw new NephosStorageDataCorruptionException(string.Format(cultureInfo, "Error decoding service metadata for blob {0}", objArray), metadataFormatException2);
			}
			blobProperty.ContentEncoding = nameValueCollection[RealServiceManager.ContentEncodingTag];
			blobProperty.ContentLanguage = nameValueCollection[RealServiceManager.ContentLanguageTag];
			blobProperty.CacheControl = nameValueCollection[RealServiceManager.CacheControlTag];
			blobProperty.CopyId = nameValueCollection[RealServiceManager.CopyIdTag];
			blobProperty.CopySource = nameValueCollection[RealServiceManager.CopySourceTag];
			blobProperty.CopyStatus = nameValueCollection[RealServiceManager.CopyStatusTag];
			blobProperty.CopyStatusDescription = nameValueCollection[RealServiceManager.CopyStatusDescriptionTag];
			blobProperty.ContentDisposition = nameValueCollection[RealServiceManager.ContentDispositionTag];
			blobProperty.DiskTag = nameValueCollection[RealServiceManager.DiskTag];
			blobProperty.DiskResourceUri = nameValueCollection[RealServiceManager.DiskResourceUri];
			if (!string.IsNullOrEmpty(nameValueCollection[RealServiceManager.CopyProgressOffsetTag]) && !string.IsNullOrEmpty(nameValueCollection[RealServiceManager.CopyProgressTotalTag]))
			{
				blobProperty.CopyProgress = string.Format("{0}/{1}", nameValueCollection[RealServiceManager.CopyProgressOffsetTag], nameValueCollection[RealServiceManager.CopyProgressTotalTag]);
			}
			blobProperty.CopyCompletionTime = RealServiceManager.ParseDateTimeFromString(nameValueCollection[RealServiceManager.CopyCompletionTimeTag]);
			long? contentLength = blobObject.ContentLength;
			NephosAssertionException.Assert(contentLength.HasValue, "Content length of blob {0} is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), resourceAccount, resourceContainer, resourceBlob);
			blobProperty.ContentLength = blobObject.ContentLength.Value;
			blobProperty.ContentType = blobObject.ContentType;
			DateTime? lastModificationTime = blobObject.LastModificationTime;
			NephosAssertionException.Assert(lastModificationTime.HasValue, "Last modification time of blob {0} is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), resourceAccount, resourceContainer, resourceBlob);
			blobProperty.LastModifiedTime = new DateTime?(blobObject.LastModificationTime.Value);
			flag = (blobObject.Type == BlobType.ListBlob || blobObject.Type == BlobType.AppendBlob ? true : blobObject.Type == BlobType.IndexBlob);
			object[] type = new object[] { blobObject.Type };
			NephosAssertionException.Assert(flag, "Invalid BlobType {0} encountered.", type);
			blobProperty.BlobType = blobObject.Type;
			if (supportCrc64)
			{
				blobProperty.ContentCrc64 = this.GetCrc64FromServiceMetadata(nameValueCollection);
			}
			bool flag1 = false;
			isMD5MigrationRequired = false;
			blobProperty.ContentMD5 = this.GetMD5FromServiceMetadata(nameValueCollection, out flag1);
			if (blobObject.Type == BlobType.ListBlob && blobProperty.ContentMD5 == null && !flag1)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("ExtractBlobProperties >> Internal MD5 migration required (isEmptyMD5 = {0}).", new object[] { flag1 });
				blobProperty.ContentMD5 = blobObject.ContentMD5;
				isMD5MigrationRequired = true;
			}
			if (blobObject.Snapshot == DateTime.MaxValue)
			{
				NephosAssertionException.Assert(blobObject.LeaseInfo != null, "Blob's lease info not populated for a root blob!.");
				blobProperty.LeaseInfo = blobObject.LeaseInfo;
			}
			blobProperty.SequenceNumber = blobObject.SequenceNumber;
			blobProperty.CreationTime = blobObject.CreationTime;
			blobProperty.CommittedBlockCount = blobObject.CommittedBlockCount;
			blobProperty.IsIncrementalCopy = blobObject.IsIncrementalCopy;
			if (blobProperty.IsIncrementalCopy)
			{
				blobProperty.LastCopySnapshot = RealServiceManager.ParseDateTimeFromString(nameValueCollection[RealServiceManager.LastCopySnapshotTag]);
			}
			blobProperty.IsBlobEncrypted = blobObject.IsBlobEncrypted;
			return blobProperty;
		}

		public IContainerProperties ExtractContainerProperties(string resourceAccount, string resourceContainer, IBaseBlobContainer baseBlobContainer, ContainerPropertyNames propertyNames)
		{
			if (baseBlobContainer == null)
			{
				throw new ArgumentNullException("container");
			}
			ContainerProperties containerProperty = new ContainerProperties();
			if ((propertyNames & ContainerPropertyNames.ApplicationMetadata) != ContainerPropertyNames.None && baseBlobContainer.ApplicationMetadata != null)
			{
				try
				{
					MetadataEncoding.Decode(baseBlobContainer.ApplicationMetadata, containerProperty.ContainerMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] resourceString = new object[] { RealServiceManager.GetResourceString(resourceAccount, resourceContainer) };
					throw new NephosStorageDataCorruptionException(string.Format(invariantCulture, "Error decoding application metadata for container {0}", resourceString), metadataFormatException);
				}
			}
			DateTime? lastModificationTime = baseBlobContainer.LastModificationTime;
			NephosAssertionException.Assert(lastModificationTime.HasValue, "Last modification time of container {0} is not known", new Func<string, string, string>(RealServiceManager.GetResourceString), resourceAccount, resourceContainer);
			containerProperty.LastModifiedTime = baseBlobContainer.LastModificationTime.Value;
			if ((propertyNames & ContainerPropertyNames.ServiceMetadata) != ContainerPropertyNames.None && baseBlobContainer.ServiceMetadata != null)
			{
				try
				{
					containerProperty.ContainerAcl = new ContainerAclSettings(baseBlobContainer.ServiceMetadata);
				}
				catch (MetadataFormatException metadataFormatException3)
				{
					MetadataFormatException metadataFormatException2 = metadataFormatException3;
					throw new NephosStorageDataCorruptionException(string.Format("Error decoding Acl setting for container {0}", RealServiceManager.GetResourceString(resourceAccount, resourceContainer)), metadataFormatException2);
				}
			}
			if ((propertyNames & ContainerPropertyNames.LeaseType) != ContainerPropertyNames.None)
			{
				containerProperty.LeaseInfo = baseBlobContainer.LeaseInfo;
			}
			return containerProperty;
		}

		private IEnumerator<IAsyncResult> GetBlobImpl(IAccountIdentifier identifier, string account, string container, string blob, Stream outputStream, BlobRegion blobRegion, bool isCalculatingCrc64ForRange, bool isCalculatingMD5ForRange, BlobPropertyNames additionalPropertyNames, BlobObjectCondition blobObjectCondition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptBlobProperties interceptBlobPropertiesCallback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeCrc64 interceptRangeCrc64Callback, Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.ServiceManager.InterceptRangeMD5 interceptRangeMD5Callback, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			DateTime dateTime;
			bool flag;
			bool flag1;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (outputStream == null)
			{
				throw new ArgumentNullException("outputStream", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetBlob");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Read, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			BlobServiceVersion blobServiceVersion = BlobServiceVersion.Original;
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IBlobObject blobObject = blobContainer1.CreateBaseBlobObjectInstance(str, dateTime, blobServiceVersion))
				{
					BlobPropertyNames blobPropertyName = BlobPropertyNames.None;
					if (interceptBlobPropertiesCallback != null)
					{
						blobPropertyName = BlobPropertyNames.ContentType | BlobPropertyNames.ContentLength | BlobPropertyNames.LastModificationTime | BlobPropertyNames.ServiceMetadata | BlobPropertyNames.BlobType | BlobPropertyNames.SequenceNumber | BlobPropertyNames.Flags | additionalPropertyNames;
						if (!excludeNonSystemHeaders)
						{
							blobPropertyName |= BlobPropertyNames.ApplicationMetadata;
						}
						if (!snapshot.HasValue)
						{
							blobPropertyName |= BlobPropertyNames.LeaseType;
						}
					}
					CrcStream crcStream = null;
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginGetBlob(blobRegion, blobPropertyName, blobObjectCondition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
					yield return asyncResult;
					crcStream = blobObject.EndGetBlob(asyncResult);
					bool flag2 = false;
					using (crcStream)
					{
						bool flag3 = true;
						long? nullable1 = null;
						if (interceptBlobPropertiesCallback != null)
						{
							IBlobProperties blobProperty = this.ExtractBlobProperties(account, container, blob, blobObject, true, out flag2);
							timeout = this.AdjustTimeoutBasedOnSizeForGetBlob(blobProperty.ContentLength, blobRegion, timeout);
							IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
							object[] totalMilliseconds = new object[] { timeout.TotalMilliseconds / 1000 };
							verbose.Log("Adjusted GetBlob timeout after determining the size of the request to {0}s", totalMilliseconds);
							if (interceptBlobPropertiesCallback(blobProperty))
							{
								nullable1 = new long?(blobProperty.ContentLength);
							}
							else
							{
								flag3 = false;
							}
						}
						if (!flag3)
						{
							goto Label0;
						}
						long value = 9223372036854775807L;
						int num = this.config.StreamCopyBufferSize;
						AsyncCrcStreamCopy.ComputeAndLogCrcMethod computeAndLogCrcMethod = null;
						bool flag4 = true;
						if (nullable1.HasValue)
						{
							long? nullable2 = nullable1;
							long num1 = (long)num;
							flag = (nullable2.GetValueOrDefault() >= num1 ? false : nullable2.HasValue);
							if (flag)
							{
								num = (int)nullable1.Value;
							}
							value = nullable1.Value;
							if (blobRegion == null)
							{
								flag1 = (isCalculatingCrc64ForRange ? false : !isCalculatingMD5ForRange);
								NephosAssertionException.Assert(flag1, "A range must be specified to be calculating a range CRC or MD5.");
							}
							else
							{
								value = blobRegion.Length;
								if (blobRegion.Offset + blobRegion.Length > nullable1.Value)
								{
									value = nullable1.Value - blobRegion.Offset;
								}
							}
						}
						else
						{
							NephosAssertionException.Assert(!flag4, "Blob must have valid content length.");
						}
						if ((isCalculatingCrc64ForRange || isCalculatingMD5ForRange) && value != 9223372036854775807L)
						{
							BufferWrapper buffer = BufferPool.GetBuffer((int)value);
							try
							{
								using (CrcMemoryStream crcMemoryStream = new CrcMemoryStream(buffer.Buffer, 0, (int)value))
								{
									if (value < (long)num)
									{
										num = (int)value;
									}
									if (!isCalculatingCrc64ForRange)
									{
										using (MD5WriterStream mD5WriterStream = new MD5WriterStream(crcMemoryStream, value, false, true))
										{
											Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Begin buffering blob data into memory");
											asyncResult = AsyncCrcStreamCopy.BeginAsyncStreamCopy(crcStream, mD5WriterStream, value, num, computeAndLogCrcMethod, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
											yield return asyncResult;
											AsyncCrcStreamCopy.EndAsyncStreamCopy(asyncResult);
											Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Completed buffering blob data into memory");
											if (interceptRangeMD5Callback == null || value <= (long)0)
											{
												goto Label1;
											}
											interceptRangeMD5Callback(mD5WriterStream.Hash);
										}
									}
									else
									{
										Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Begin buffering blob data into memory from CrcMemoryStream");
										asyncResult = AsyncCrcStreamCopy.BeginAsyncStreamCopy(crcStream, crcMemoryStream, value, num, computeAndLogCrcMethod, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
										yield return asyncResult;
										AsyncCrcStreamCopy.EndAsyncStreamCopy(asyncResult);
										Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Completed buffering blob data into memory from CrcMemoryStream");
										long num2 = (long)0;
										if (value > (long)0)
										{
											num2 = CrcUtils.ComputeCrc(buffer.Buffer, 0, (int)value);
										}
										if (interceptRangeCrc64Callback != null && value > (long)0)
										{
											interceptRangeCrc64Callback(new long?(num2));
										}
									}
								Label1:
									if (crcMemoryStream.DataCrc.HasValue)
									{
										long num3 = CrcUtils.ComputeCrc(buffer.Buffer, 0, (int)value);
										if (num3 != crcMemoryStream.DataCrc.Value)
										{
											throw new CrcMismatchException("Hit CRC mismatch after FE cached the data and calculated MD5.", crcMemoryStream.DataCrc.Value, num3, true);
										}
										IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
										object[] length = new object[] { crcMemoryStream.Length, num3 };
										stringDataEventStream.Log("Data CRC after FE cached data and MD5 calculation. DataLength:{0} CRC:0x{1:x}", length);
									}
									asyncResult = outputStream.BeginWrite(buffer.Buffer, 0, (int)value, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
									yield return asyncResult;
									outputStream.EndWrite(asyncResult);
									Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Blob data transmitted back to client");
								}
							}
							finally
							{
								BufferPool.ReleaseBuffer(buffer);
							}
						}
						else
						{
							using (CrcVerifierStream crcVerifierStream = new CrcVerifierStream(outputStream, false))
							{
								asyncResult = AsyncCrcStreamCopy.BeginAsyncStreamCopy(crcStream, crcVerifierStream, value, num, computeAndLogCrcMethod, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobImpl"));
								yield return asyncResult;
								AsyncCrcStreamCopy.EndAsyncStreamCopy(asyncResult);
							}
						}
					}
				}
			}
		Label0:
			yield break;
		}

		private IEnumerator<IAsyncResult> GetBlobMetadataImpl(IAccountIdentifier identifier, string account, string container, string blob, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AsyncHelper.Tuple<DateTime, NameValueCollection>> context)
		{
			DateTime dateTime;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetBlobMetadata");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobMetadataImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IBlobObject blobObject = blobContainer1.CreateBaseBlobObjectInstance(str, dateTime))
				{
					BlobPropertyNames blobPropertyName = BlobPropertyNames.LastModificationTime | BlobPropertyNames.ApplicationMetadata | BlobPropertyNames.Flags;
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginGetProperties(blobPropertyName, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobMetadata"));
					yield return asyncResult;
					blobObject.EndGetProperties(asyncResult);
					context.ResultData = new AsyncHelper.Tuple<DateTime, NameValueCollection>();
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "Last modification time of blob {0} is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					context.ResultData.First = blobObject.LastModificationTime.Value;
					context.ResultData.Second = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
					if (blobObject.ApplicationMetadata == null)
					{
						goto Label0;
					}
					try
					{
						MetadataEncoding.Decode(blobObject.ApplicationMetadata, context.ResultData.Second);
					}
					catch (MetadataFormatException metadataFormatException1)
					{
						MetadataFormatException metadataFormatException = metadataFormatException1;
						throw new NephosStorageDataCorruptionException(string.Format("Error decoding application metadata for blob {0}", RealServiceManager.GetResourceString(account, container, blob)), metadataFormatException);
					}
				}
			}
		Label0:
			yield break;
		}

		private IEnumerator<IAsyncResult> GetBlobPropertiesImpl(IAccountIdentifier identifier, string account, string container, string blob, bool supportCrc64, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, bool excludeNonSystemHeaders, RequestContext requestContext, AsyncIteratorContext<IBlobProperties> context)
		{
			DateTime dateTime;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetBlobProperties");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Read, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobPropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			BlobServiceVersion blobServiceVersion = BlobServiceVersion.Original;
			if (requestContext.IsRequestVersionAtLeastFebruary12)
			{
				blobServiceVersion = BlobServiceVersion.Feb12;
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IBlobObject blobObject = blobContainer1.CreateBaseBlobObjectInstance(str, dateTime, blobServiceVersion))
				{
					BlobPropertyNames blobPropertyName = BlobPropertyNames.ContentType | BlobPropertyNames.ContentLength | BlobPropertyNames.LastModificationTime | BlobPropertyNames.ServiceMetadata | BlobPropertyNames.BlobType | BlobPropertyNames.SequenceNumber | BlobPropertyNames.Flags;
					if (!excludeNonSystemHeaders)
					{
						blobPropertyName |= BlobPropertyNames.ApplicationMetadata;
					}
					if (!snapshot.HasValue)
					{
						blobPropertyName |= BlobPropertyNames.LeaseType;
					}
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginGetProperties(blobPropertyName, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobProperties"));
					yield return asyncResult;
					blobObject.EndGetProperties(asyncResult);
					bool flag = false;
					IBlobProperties blobProperty = this.ExtractBlobProperties(account, container, blob, blobObject, supportCrc64, out flag);
					context.ResultData = blobProperty;
				}
			}
		}

		private IEnumerator<IAsyncResult> GetBlobServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AnalyticsSettings> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetBlobServiceProperties");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobServiceProperties"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)32768)), null, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobServiceProperties"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.BlobAnalyticsSettings;
		}

		private IEnumerator<IAsyncResult> GetBlobServiceStatsImpl(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<GeoReplicationStats> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetBlobServiceStats");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Read | PermissionLevel.Owner, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobServiceStats"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(ownerAccountName);
			storageAccount.Timeout = timeout;
			AccountCondition accountCondition = new AccountCondition(false, false, storageAccount.LastModificationTime, null);
			asyncResult = storageAccount.BeginGetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)268435456)), null, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlobServiceStats"));
			yield return asyncResult;
			storageAccount.EndGetProperties(asyncResult);
			context.ResultData = storageAccount.ServiceMetadata.BlobGeoReplicationStats;
		}

		private IEnumerator<IAsyncResult> GetBlockListImpl(IAccountIdentifier identifier, string account, string container, string blob, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, BlobObjectCondition condition, DateTime? snapshot, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IBlockLists> context)
		{
			DateTime dateTime;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			PermissionLevel permissionLevel = PermissionLevel.Read;
			if ((blockListTypes & BlockListTypes.Uncommitted) != BlockListTypes.None)
			{
				permissionLevel |= PermissionLevel.Owner;
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, permissionLevel, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlockListImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IListBlobObject listBlobObject = blobContainer1.CreateBlockBlobInstance(str, dateTime, blobServiceVersion))
				{
					listBlobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = listBlobObject.BeginGetBlockList(condition, blockListTypes, blobServiceVersion, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetBlockListImpl"));
					yield return asyncResult;
					IBlockLists value = listBlobObject.EndGetBlockList(asyncResult);
					NephosAssertionException.Assert(listBlobObject.LastModificationTime.HasValue, "The blob's LMT must be returned for GetBlockList.");
					NephosAssertionException.Assert(listBlobObject.ContentLength.HasValue, "The blob's size must be returned for GetBlockList.");
					value.BlobLastModificationTime = listBlobObject.LastModificationTime.Value;
					value.BlobSize = listBlobObject.ContentLength.Value;
					context.ResultData = value;
				}
			}
		}

		private IEnumerator<IAsyncResult> GetContainerAclImpl(IAccountIdentifier identifier, string account, string container, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<AsyncHelper.Tuple<DateTime, ContainerAclSettings>> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetContainerAcl");
			}
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.ReadAcl, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerAclImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				ContainerPropertyNames containerPropertyName = ContainerPropertyNames.LastModificationTime | ContainerPropertyNames.ServiceMetadata;
				ContainerCondition containerCondition = null;
				if (leaseId.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						LeaseId = leaseId
					};
				}
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginGetProperties(containerPropertyName, containerCondition, CacheRefreshOptions.SkipAllCache, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerAclImpl"));
				yield return asyncResult;
				blobContainer.EndGetProperties(asyncResult);
				context.ResultData = new AsyncHelper.Tuple<DateTime, ContainerAclSettings>();
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "Last modification time of container {0} is not known", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				context.ResultData.First = blobContainer.LastModificationTime.Value;
				try
				{
					context.ResultData.Second = new ContainerAclSettings(blobContainer.ServiceMetadata);
				}
				catch (MetadataFormatException metadataFormatException1)
				{
					MetadataFormatException metadataFormatException = metadataFormatException1;
					throw new NephosStorageDataCorruptionException(string.Format("Error decoding Acl setting for container {0}", RealServiceManager.GetResourceString(account, container)), metadataFormatException);
				}
			}
		}

		private IEnumerator<IAsyncResult> GetContainerMetadataImpl(IAccountIdentifier identifier, string accountName, string containerName, DateTime? snapshot, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IContainerProperties> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(containerName))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetContainerMetadata");
			}
			if (snapshot.HasValue)
			{
				throw new NotSupportedException("Blob containers do not support snapshots yet.");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, containerName, null, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerMetadata"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBaseBlobContainer operationStatus = (IBaseBlobContainer)this.storageManager.CreateContainerInstance(accountName, containerName, requestContext.ServiceType))
			{
				if (requestContext != null)
				{
					operationStatus.OperationStatus = requestContext.OperationStatus;
				}
				ContainerPropertyNames containerPropertyName = ContainerPropertyNames.LastModificationTime | ContainerPropertyNames.ServiceMetadata | ContainerPropertyNames.ApplicationMetadata;
				ContainerCondition containerCondition = null;
				if (ifModifiedSince.HasValue || leaseId.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						IfModifiedSinceTime = ifModifiedSince,
						LeaseId = leaseId
					};
				}
				operationStatus.Timeout = startingNow.Remaining(timeout);
				asyncResult = operationStatus.BeginGetProperties(containerPropertyName, containerCondition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerMetadata"));
				yield return asyncResult;
				operationStatus.EndGetProperties(asyncResult);
				context.ResultData = this.ExtractContainerProperties(accountName, containerName, operationStatus, containerPropertyName);
			}
		}

		private IEnumerator<IAsyncResult> GetContainerPropertiesImpl(IAccountIdentifier identifier, string accountName, string containerName, DateTime? snapshot, DateTime? ifModifiedSinceTime, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IContainerProperties> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account");
			}
			if (string.IsNullOrEmpty(containerName))
			{
				throw new ArgumentException("container");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in GetContainerProperties");
			}
			if (snapshot.HasValue)
			{
				throw new NotSupportedException("Blob containers do not support snapshots yet.");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, containerName, null, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerPropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			string str = accountName;
			string str1 = containerName;
			using (IBaseBlobContainer operationStatus = (IBaseBlobContainer)this.storageManager.CreateContainerInstance(str, str1, requestContext.ServiceType))
			{
				if (requestContext != null)
				{
					operationStatus.OperationStatus = requestContext.OperationStatus;
				}
				ContainerPropertyNames containerPropertyName = ContainerPropertyNames.Common;
				ContainerCondition containerCondition = null;
				if (ifModifiedSinceTime.HasValue || leaseId.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						IfModifiedSinceTime = ifModifiedSinceTime,
						LeaseId = leaseId
					};
				}
				operationStatus.Timeout = startingNow.Remaining(timeout);
				asyncResult = operationStatus.BeginGetProperties(containerPropertyName, containerCondition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetContainerPropertiesImpl"));
				yield return asyncResult;
				operationStatus.EndGetProperties(asyncResult);
				context.ResultData = this.ExtractContainerProperties(str, str1, operationStatus, containerPropertyName);
			}
		}

		public static string GetCopySourceRequestVersion(RequestContext requestContext)
		{
			return VersioningConfigurationLookup.Instance.LatestVersion;
		}

		protected long? GetCrc64FromServiceMetadata(NameValueCollection serviceMetadata)
		{
			long? nullable = null;
			string item = serviceMetadata[RealServiceManager.ContentCrc64Tag];
			if (!string.IsNullOrEmpty(item))
			{
				nullable = new long?(BitConverter.ToInt64(Convert.FromBase64String(item), 0));
			}
			return nullable;
		}

		public virtual ContainerPropertyNames GetListContainerPropertyNames()
		{
			return ContainerPropertyNames.Common;
		}

		private static TimeSpan GetMaxTimeoutForCopySourceRequest(TimeSpan timeout)
		{
			if (RealServiceManager.MaxAllowedTimeoutToGetResponseFromCopySource >= timeout)
			{
				return timeout;
			}
			return RealServiceManager.MaxAllowedTimeoutToGetResponseFromCopySource;
		}

		protected byte[] GetMD5FromServiceMetadata(NameValueCollection serviceMetadata, out bool isEmptyMD5)
		{
			byte[] numArray = null;
			isEmptyMD5 = false;
			string item = serviceMetadata[RealServiceManager.ContentMD5Tag];
			if (!string.IsNullOrEmpty(item))
			{
				if (item == RealServiceManager.EmptyContentMD5Value)
				{
					isEmptyMD5 = true;
				}
				else
				{
					numArray = Convert.FromBase64String(item);
				}
			}
			return numArray;
		}

		private IEnumerator<IAsyncResult> GetPageRangeListImpl(IAccountIdentifier identifier, string account, string container, string blob, BlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, BlobObjectCondition condition, DateTime? snapshot, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IGetPageRangeListResult> context)
		{
			DateTime dateTime;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Read
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetPageRangeListImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				IBlobContainer blobContainer1 = blobContainer;
				string str = blob;
				DateTime? nullable = snapshot;
				dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
				using (IIndexBlobObject indexBlobObject = blobContainer1.CreateIndexBlobObjectInstance(str, dateTime))
				{
					if (!requestContext.IsRequestVersionAtLeastJuly15)
					{
						indexBlobObject.Timeout = startingNow.Remaining(timeout);
						asyncResult = indexBlobObject.BeginGetPageRangeList(blobRegion, additionalPropertyNames, condition, maxPageRanges, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetPageRangeListImpl"));
						yield return asyncResult;
					}
					else
					{
						indexBlobObject.Timeout = startingNow.Remaining(timeout);
						asyncResult = indexBlobObject.BeginGetPageRangeList(blobRegion, additionalPropertyNames, condition, maxPageRanges, prevSnapshotTimestamp, isRangeCompressed, skipClearPages, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.GetPageRangeListImpl"));
						yield return asyncResult;
					}
					IPageRangeCollection pageRangeCollections = indexBlobObject.EndGetPageRangeList(asyncResult);
					NephosAssertionException.Assert(indexBlobObject.LastModificationTime.HasValue, "The blob's LMT must be returned for GetPageRangeList.");
					NephosAssertionException.Assert(indexBlobObject.ContentLength.HasValue, "The blob's size must be returned for GetPageRangeList.");
					GetPageRangeListResult getPageRangeListResult = new GetPageRangeListResult(pageRangeCollections)
					{
						BlobLastModificationTime = indexBlobObject.LastModificationTime.Value,
						ContentLength = indexBlobObject.ContentLength.Value
					};
					context.ResultData = getPageRangeListResult;
				}
			}
		}

		public static string GetResourceString(string account, string container, string blob)
		{
			string[] empty = new string[] { string.Empty, account, container, blob };
			return string.Join("/", empty);
		}

		public static string GetResourceString(string account, string container)
		{
			string[] empty = new string[] { string.Empty, account, container };
			return string.Join("/", empty);
		}

		public static string GetResourceString(string account)
		{
			return string.Concat("/", account);
		}

		private static IEnumerator<IAsyncResult> GetResponseFromCopySourceImpl(string requestMethod, UriString copySource, string requestVersion, BlobObjectCondition sourceCondition, TimeSpan timeout, AsyncIteratorContext<HttpWebResponse> context)
		{
			Duration startingNow = Duration.StartingNow;
			HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(copySource.RawString);
			httpWebRequest.Method = requestMethod;
			httpWebRequest.ContentLength = (long)0;
			httpWebRequest.Headers["x-ms-version"] = requestVersion;
			RealServiceManager.ApplySourceConditions(httpWebRequest, sourceCondition);
			IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
			object[] method = new object[] { httpWebRequest.Method, copySource.SafeStringForLogging };
			verbose.Log("FE CopyBlob: attempting to get response to {0} request from copy source: {1}", method);
			IAsyncResult asyncResult = httpWebRequest.BeginGetResponse(context.GetResumeCallback(), context.GetResumeState("BeginGetResponse"));
			HttpWebRequest httpWebRequest1 = httpWebRequest;
			TimeSpan timeSpan = startingNow.Remaining(timeout);
			HttpUtilities.AbortHttpWebRequestOnTimeout(httpWebRequest1, (int)timeSpan.TotalMilliseconds, asyncResult);
			yield return asyncResult;
			HttpWebResponse httpWebResponse = null;
			try
			{
				httpWebResponse = (HttpWebResponse)httpWebRequest.EndGetResponse(asyncResult);
				IStringDataEventStream stringDataEventStream = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] objArray = new object[] { httpWebResponse.Method, null };
				object[] objArray1 = objArray;
				string item = httpWebResponse.Headers["x-ms-request-id"];
				if (item == null)
				{
					item = "NULL";
				}
				objArray1[1] = item;
				stringDataEventStream.Log("FE CopyBlob: Received {0} response from copy source. x-ms-request-id: {1}", objArray);
			}
			catch (WebException webException1)
			{
				WebException webException = webException1;
				httpWebResponse = webException.Response as HttpWebResponse;
				if (httpWebResponse == null)
				{
					IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
					object[] method1 = new object[] { httpWebRequest.Method, webException.Status, webException.Message };
					error.Log("FE CopyBlob: Hit WebException while getting response to {0} request, no response was received from copy source. WebExceptionStatus: {1}. Message: {2}", method1);
					throw new CannotVerifyCopySourceException();
				}
				IStringDataEventStream error1 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] method2 = new object[] { httpWebResponse.Method, webException.Status, webException.Message, null };
				object[] objArray2 = method2;
				string str = httpWebResponse.Headers["x-ms-request-id"];
				if (str == null)
				{
					str = "NULL";
				}
				objArray2[3] = str;
				error1.Log("FE CopyBlob: Received {0} response from copy source but hit WebException. WebExceptionStatus: {1}. Message: {2}. x-ms-request-id: {3}", method2);
			}
			catch (IOException oException1)
			{
				IOException oException = oException1;
				IStringDataEventStream stringDataEventStream1 = Logger<IRestProtocolHeadLogger>.Instance.Error;
				object[] method3 = new object[] { httpWebRequest.Method, oException.Message };
				stringDataEventStream1.Log("FE CopyBlob: Hit IOException while getting response to {0} request: {1}", method3);
				throw new CannotVerifyCopySourceException();
			}
			context.ResultData = httpWebResponse;
		}

		private static long? GetSequenceNumberFromHeader(string sequenceNumberString)
		{
			long? nullable = null;
			if (!string.IsNullOrEmpty(sequenceNumberString))
			{
				try
				{
					nullable = new long?(long.Parse(sequenceNumberString));
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("FE CopyBlob: FormatException while parsing sequence number: {0}", new object[] { formatException.Message });
					string str = string.Format("Copy source header {0} value {1} must be an integer.", "x-ms-blob-sequence-number", sequenceNumberString);
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, str, formatException);
				}
			}
			if (nullable.HasValue)
			{
				long? nullable1 = nullable;
				if ((nullable1.GetValueOrDefault() >= (long)0 ? false : nullable1.HasValue))
				{
					throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, string.Format("Copy source header {0} value {1} must be greater than 0.", "x-ms-blob-sequence-number", sequenceNumberString));
				}
			}
			return nullable;
		}

		private byte[] GetServiceMetadataFromProperties(string resource, IPutBlobProperties properties, bool supportCrc64, bool generationIdEnabled)
		{
			byte[] numArray;
			string str;
			NameValueCollection nameValueCollection = new NameValueCollection();
			RealServiceManager.AddServiceProperty(resource, nameValueCollection, RealServiceManager.ContentEncodingTag, properties.ContentEncoding);
			RealServiceManager.AddServiceProperty(resource, nameValueCollection, RealServiceManager.ContentLanguageTag, properties.ContentLanguage);
			RealServiceManager.AddServiceProperty(resource, nameValueCollection, RealServiceManager.CacheControlTag, properties.CacheControl);
			RealServiceManager.AddServiceProperty(resource, nameValueCollection, RealServiceManager.ContentDispositionTag, properties.ContentDisposition);
			if (supportCrc64)
			{
				RealServiceManager.SetServiceProperty(resource, nameValueCollection, RealServiceManager.ContentCrc64Tag, properties.ContentCrc64.ToBase64String());
			}
			str = (properties.ContentMD5 == null ? RealServiceManager.EmptyContentMD5Value : Convert.ToBase64String(properties.ContentMD5));
			RealServiceManager.AddServiceProperty(resource, nameValueCollection, RealServiceManager.ContentMD5Tag, str);
			try
			{
				numArray = MetadataEncoding.Encode(nameValueCollection);
			}
			catch (MetadataFormatException metadataFormatException1)
			{
				MetadataFormatException metadataFormatException = metadataFormatException1;
				object[] objArray = new object[] { resource, metadataFormatException.Message };
				NephosAssertionException.Fail(metadataFormatException, "Failed to encode service metadata for blob {0}: {1}", objArray);
				return null;
			}
			return numArray;
		}

		private IEnumerator<IAsyncResult> ListBlobsImpl(IAccountIdentifier identifier, string accountName, string container, BlobServiceVersion version, string blobPrefix, string delimiter, string containerMarker, string blobMarker, DateTime? snapshotMarker, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, bool includeDisabledContainers, bool isFetchingMetadata, bool isIncludingSnapshots, bool isIncludingPageBlobs, bool isIncludingAppendBlobs, bool isIncludingUncommittedBlobs, bool isIncludingLeaseStatus, int? maxBlobNames, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IListBlobsResultCollection> context)
		{
			int num;
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account");
			}
			if (maxBlobNames.HasValue)
			{
				int? nullable = maxBlobNames;
				flag = (nullable.GetValueOrDefault() >= 1 ? false : nullable.HasValue);
				if (flag)
				{
					throw new ArgumentOutOfRangeException("maxBlobNames", "Must be either null or a positive value");
				}
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ListBlobs");
			}
			BlobPropertyNames blobPropertyName = BlobPropertyNames.ContentType | BlobPropertyNames.ContentLength | BlobPropertyNames.LastModificationTime | BlobPropertyNames.ServiceMetadata | BlobPropertyNames.BlobType | BlobPropertyNames.SequenceNumber | BlobPropertyNames.Flags;
			if (isFetchingMetadata)
			{
				blobPropertyName |= BlobPropertyNames.ApplicationMetadata;
			}
			if (isIncludingLeaseStatus)
			{
				blobPropertyName |= BlobPropertyNames.LeaseType;
			}
			BlobObjectCondition blobObjectCondition = new BlobObjectCondition()
			{
				IfModifiedSinceTime = ifModifiedSinceTime,
				IfNotModifiedSinceTime = ifNotModifiedSinceTime,
				IncludeDisabledContainers = includeDisabledContainers,
				IsFetchingMetadata = isFetchingMetadata,
				IsIncludingSnapshots = isIncludingSnapshots,
				IsIncludingUncommittedBlobs = isIncludingUncommittedBlobs,
				IsIncludingPageBlobs = isIncludingPageBlobs,
				IsIncludingAppendBlobs = isIncludingAppendBlobs,
				IsOperationAllowedOnArchivedBlobs = true
			};
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.List
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, container, null, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ListBlobsImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(accountName, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				IBlobContainer blobContainer1 = blobContainer;
				string str = blobPrefix;
				BlobPropertyNames blobPropertyName1 = blobPropertyName;
				string str1 = delimiter;
				string str2 = blobMarker;
				DateTime? nullable1 = snapshotMarker;
				BlobObjectCondition blobObjectCondition1 = blobObjectCondition;
				int? nullable2 = maxBlobNames;
				num = (nullable2.HasValue ? nullable2.GetValueOrDefault() : 5000);
				asyncResult = blobContainer1.BeginListBlobs(str, blobPropertyName1, str1, str2, nullable1, blobObjectCondition1, num, version, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ListBlobs"));
				yield return asyncResult;
				IBlobObjectCollection blobObjectCollections = blobContainer.EndListBlobs(asyncResult);
				context.ResultData = new ListBlobsResult(delimiter, blobObjectCollections, blobContainer, version);
				blobContainer = null;
			}
		}

		private IEnumerator<IAsyncResult> ListContainersImpl(IAccountIdentifier identifier, string accountName, string containerPrefix, string delimiter, string marker, bool includeDisabledContainers, bool includeSnapshots, bool isFetchingLeaseInfo, DateTime? ifModifiedSinceTime, DateTime? ifNotModifiedSinceTime, int? maxContainerNames, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IListContainersResultCollection> context)
		{
			int num;
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account");
			}
			if (maxContainerNames.HasValue)
			{
				int? nullable = maxContainerNames;
				flag = (nullable.GetValueOrDefault() >= 1 ? false : nullable.HasValue);
				if (flag)
				{
					throw new ArgumentOutOfRangeException("maxContainerNames", "Must be either null or positive");
				}
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ListContainers");
			}
			if (includeSnapshots && requestContext.ServiceType != ServiceType.FileService)
			{
				throw new NotSupportedException("Only file containers support snapshots for now.");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.List
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, null, null, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ListContainersImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			ContainerPropertyNames listContainerPropertyNames = this.GetListContainerPropertyNames();
			ContainerCondition containerCondition = new ContainerCondition()
			{
				IncludeDisabledContainers = includeDisabledContainers,
				IncludeSnapshots = includeSnapshots,
				IfModifiedSinceTime = ifModifiedSinceTime,
				IfNotModifiedSinceTime = ifNotModifiedSinceTime
			};
			using (IStorageAccount storageAccount = this.storageManager.CreateAccountInstance(accountName))
			{
				storageAccount.Timeout = startingNow.Remaining(timeout);
				IStorageAccount storageAccount1 = storageAccount;
				ServiceType serviceType = requestContext.ServiceType;
				string str = containerPrefix;
				if (str == null)
				{
					str = "";
				}
				ContainerPropertyNames containerPropertyName = listContainerPropertyNames;
				string str1 = delimiter;
				string str2 = marker;
				ContainerCondition containerCondition1 = containerCondition;
				int? nullable1 = maxContainerNames;
				num = (nullable1.HasValue ? nullable1.GetValueOrDefault() : 5000);
				asyncResult = storageAccount1.BeginListContainers(serviceType, str, containerPropertyName, str1, str2, containerCondition1, num, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ListContainersImpl"));
				yield return asyncResult;
				context.ResultData = new ListContainersResult(storageAccount.EndListContainers(requestContext.ServiceType, asyncResult));
				storageAccount = null;
			}
		}

		public static DateTime? ParseDateTimeFromString(string str)
		{
			DateTime? nullable = null;
			long? nullable1 = RealServiceManager.ParseLongFromString(str);
			if (nullable1.HasValue)
			{
				nullable = new DateTime?(DateTime.FromFileTimeUtc(nullable1.Value));
			}
			return nullable;
		}

		public static long? ParseLongFromString(string str)
		{
			long? nullable = null;
			try
			{
				if (!string.IsNullOrEmpty(str))
				{
					nullable = new long?(long.Parse(str));
				}
			}
			catch (Exception exception)
			{
				throw new ArgumentException("failed to parse as long", str);
			}
			return nullable;
		}

		public static DateTime ParseSoftDeleteTimeString(string str)
		{
			DateTime dateTime;
			try
			{
				DateTime dateTime1 = DateTime.ParseExact(str, "yyyy/MM/dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture, DateTimeStyles.None);
				dateTime = DateTime.SpecifyKind(dateTime1, DateTimeKind.Utc);
			}
			catch (Exception exception)
			{
				throw new ArgumentException("failed to parse as date time", str);
			}
			return dateTime;
		}

		private IEnumerator<IAsyncResult> PutBlobFromBlocksImpl(IAccountIdentifier identifier, string account, string container, string blob, BlobServiceVersion blobServiceVersion, byte[][] blockIdList, BlockSource[] blockSourceList, IPutBlobProperties putBlobProperties, BlobObjectCondition condition, OverwriteOption overwriteOption, IUpdateOptions updateOptions, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IPutBlobFromBlocksResult> context)
		{
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob");
			}
			if (blockIdList == null)
			{
				throw new ArgumentNullException("blockIdList");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in PutBlobFromBlocks");
			}
			if (blockSourceList != null)
			{
				bool length = (int)blockIdList.Length == (int)blockSourceList.Length;
				object[] objArray = new object[] { (int)blockIdList.Length, (int)blockSourceList.Length };
				NephosAssertionException.Assert(length, "The lengths of the block ID list ({0}) and block source list ({1}) must be equal.", objArray);
			}
			string resourceString = RealServiceManager.GetResourceString(account, container, blob);
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Create
			};
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IListBlobObject listBlobObject = blobContainer.CreateBlockBlobInstance(blob, blobServiceVersion))
				{
					string str = null;
					byte[] serviceMetadataFromProperties = null;
					byte[] numArray = null;
					long? nullable = null;
					byte[] numArray1 = null;
					if (updateOptions != null)
					{
						NephosAssertionException.Fail("updateOptions should always be null since it should be unused!");
						if (condition == null)
						{
							condition = new BlobObjectCondition();
						}
						condition.UpdateOptions = updateOptions;
					}
					bool flag1 = false;
					if (putBlobProperties != null)
					{
						this.ConvertPutBlobProperties(resourceString, putBlobProperties, out numArray, out str, out nullable, out numArray1);
						RealServiceManager realServiceManager = this;
						string str1 = resourceString;
						IPutBlobProperties putBlobProperty = putBlobProperties;
						long? nullable1 = putBlobProperties.ContentCrc64;
						serviceMetadataFromProperties = realServiceManager.GetServiceMetadataFromProperties(str1, putBlobProperty, nullable1.HasValue, flag1);
						StorageStampHelpers.ValidateServiceProperties(serviceMetadataFromProperties, str);
					}
					if (updateOptions != null)
					{
						if (updateOptions.ServiceMetadataOption == MetadataOption.Update && (int)serviceMetadataFromProperties.Length <= 0)
						{
							serviceMetadataFromProperties = null;
						}
						if (updateOptions.ApplicationMetadataOption == MetadataOption.Update && (int)numArray.Length <= 0)
						{
							numArray = null;
						}
					}
					long num = (long)-1;
					listBlobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = listBlobObject.BeginPutBlob(str, num, serviceMetadataFromProperties, numArray, blockIdList, blockSourceList, numArray1, overwriteOption, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.PutBlobFromBlocksImpl"));
					yield return asyncResult;
					listBlobObject.EndPutBlob(asyncResult);
					DateTime? lastModificationTime = listBlobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "PutBlob call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					PutBlobFromBlocksResult putBlobFromBlocksResult = new PutBlobFromBlocksResult()
					{
						LastModifiedTime = listBlobObject.LastModificationTime.Value
					};
					PutBlobFromBlocksResult putBlobFromBlocksResult1 = putBlobFromBlocksResult;
					flag = (listBlobObject.IsWriteEncrypted.HasValue ? listBlobObject.IsWriteEncrypted.Value : false);
					putBlobFromBlocksResult1.IsWriteEncrypted = flag;
					context.ResultData = putBlobFromBlocksResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, long contentLength, IPutBlobProperties putBlobProperties, bool supportCrc64, bool calculateCrc64, bool storeCrc64, bool verifyCrc64, bool calculateMd5, bool storeMd5, bool verifyMd5, bool generationIdEnabled, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, BlobObjectCondition condition, OverwriteOption overwriteOption, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IPutBlobResult> context)
		{
			long? maxBlobSize;
			bool flag;
			ISequenceNumberUpdate sequenceNumberUpdate;
			bool flag1;
			IPutBlobProperties putBlobProperty = putBlobProperties;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (supportCrc64 && verifyMd5 && verifyCrc64)
			{
				throw new ArgumentException("If both CRC and MD5 are sent, we only validate CRC and blindly store MD5");
			}
			if (inputStream == null)
			{
				throw new ArgumentNullException("inputStream");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in PutBlob");
			}
			string str = null;
			byte[] numArray = null;
			byte[] numArray1 = null;
			long? nullable = null;
			byte[] numArray2 = null;
			string resourceString = RealServiceManager.GetResourceString(account, container, blob);
			if (generationIdEnabled)
			{
				object obj = putBlobProperty;
				if (obj == null)
				{
					obj = new PutBlobProperties();
				}
				putBlobProperty = (IPutBlobProperties)obj;
				putBlobProperty.GenerationId = Guid.NewGuid();
			}
			if (putBlobProperty != null)
			{
				this.ConvertPutBlobProperties(resourceString, putBlobProperty, out numArray1, out str, out nullable, out numArray2);
				if ((!storeMd5 || !calculateMd5) && (!storeCrc64 || !calculateCrc64) || putBlobProperty.BlobType != BlobType.ListBlob)
				{
					numArray = this.GetServiceMetadataFromProperties(resourceString, putBlobProperty, supportCrc64, generationIdEnabled);
					RealServiceManager.AlertIfServicePropertiesExceedSizeThreshold(str, numArray);
				}
			}
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Create
			};
			if (this.authorizationManager.CheckAccessWithMultiplePermissions(identifier, account, container, blob, PermissionLevel.Write, SasType.ResourceSas | SasType.AccountSas, SasResourceType.Object, sASPermissions, startingNow.Remaining(timeout)).SignedPermission == SASPermission.Create)
			{
				this.AuthorizationCondition = new Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.AuthorizationCondition()
				{
					MustNotExist = true
				};
				this.ApplyAuthorizationConditions(base.AuthorizationCondition, condition, ref overwriteOption);
			}
			IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container);
			IBlobObject blobObject = blobContainer.CreateBlobObjectInstance(putBlobProperty.BlobType, blob);
			CrcReaderStream crcReaderStream = null;
			MD5ReaderStream mD5ReaderStream = null;
			try
			{
				mD5ReaderStream = new MD5ReaderStream(inputStream, contentLength, false, requestContext);
				if (verifyMd5)
				{
					mD5ReaderStream.HashToVerifyAgainst = numArray2;
				}
				else if (numArray2 != null)
				{
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] base64String = new object[] { Convert.ToBase64String(numArray2) };
					verbose.Log("PutBlob has MD5 request header specified: {0}. The header will not be verified.", base64String);
				}
				PutBlobResult value = new PutBlobResult();
				GeneratePutBlobServiceMetadata generatePutBlobServiceMetadatum = () => {
					if (supportCrc64 && crcReaderStream != null)
					{
						putBlobProperty.ContentCrc64 = new long?(crcReaderStream.CurrentCrc);
					}
					if (putBlobProperty.ContentMD5 == null && mD5ReaderStream != null)
					{
						putBlobProperty.ContentMD5 = mD5ReaderStream.Hash;
					}
					byte[] serviceMetadataFromProperties = this.GetServiceMetadataFromProperties(resourceString, putBlobProperty, supportCrc64, generationIdEnabled);
					RealServiceManager.AlertIfServicePropertiesExceedSizeThreshold(str, serviceMetadataFromProperties);
					return serviceMetadataFromProperties;
				};
				blobObject.Timeout = startingNow.Remaining(timeout);
				IBlobObject blobObject1 = blobObject;
				string str1 = str;
				long num = contentLength;
				if (putBlobProperty != null)
				{
					maxBlobSize = putBlobProperty.MaxBlobSize;
				}
				else
				{
					maxBlobSize = null;
				}
				byte[] numArray3 = numArray;
				byte[] numArray4 = numArray1;
				Stream stream = mD5ReaderStream;
				if (stream == null)
				{
					stream = crcReaderStream;
					if (stream == null)
					{
						stream = inputStream;
					}
				}
				CrcReaderStream crcReaderStream1 = crcReaderStream;
				if (!storeMd5 || !calculateMd5)
				{
					flag = (!storeCrc64 ? false : calculateCrc64);
				}
				else
				{
					flag = true;
				}
				GeneratePutBlobServiceMetadata generatePutBlobServiceMetadatum1 = generatePutBlobServiceMetadatum;
				bool flag2 = isLargeBlockBlobRequest;
				bool flag3 = is8TBPageBlobAllowed;
				if (putBlobProperty != null)
				{
					sequenceNumberUpdate = putBlobProperty.SequenceNumberUpdate;
				}
				else
				{
					sequenceNumberUpdate = null;
				}
				IAsyncResult asyncResult = blobObject1.BeginPutBlob(str1, num, maxBlobSize, numArray3, numArray4, stream, crcReaderStream1, null, flag, generatePutBlobServiceMetadatum1, flag2, flag3, sequenceNumberUpdate, overwriteOption, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.PutBlobImpl"));
				yield return asyncResult;
				blobObject.EndPutBlob(asyncResult);
				DateTime? lastModificationTime = blobObject.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "PutBlob call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
				value.LastModifiedTime = blobObject.LastModificationTime.Value;
				PutBlobResult putBlobResult = value;
				flag1 = (blobObject.IsWriteEncrypted.HasValue ? blobObject.IsWriteEncrypted.Value : false);
				putBlobResult.IsWriteEncrypted = flag1;
				blobObject.Dispose();
				blobContainer.Dispose();
				try
				{
					if (supportCrc64 && putBlobProperty.BlobType == BlobType.ListBlob)
					{
						value.ContentCrc64 = new long?(crcReaderStream.CalculatedCrc);
					}
					if (mD5ReaderStream != null)
					{
						value.ContentMD5 = mD5ReaderStream.Hash;
					}
					else if (putBlobProperty.ContentMD5 != null)
					{
						value.ContentMD5 = putBlobProperty.ContentMD5;
					}
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					object[] objArray = new object[] { RealServiceManager.GetResourceString(account, container, blob) };
					NephosAssertionException.Fail(invalidOperationException, "PutBlob call for blob {0} completed successfully, however MD5 hash of blob contents is not available, which indicates that not all of the blob contents were read during the PutBlob call.", objArray);
				}
				context.ResultData = value;
			}
			finally
			{
				if (mD5ReaderStream != null)
				{
					((IDisposable)mD5ReaderStream).Dispose();
				}
				if (crcReaderStream != null)
				{
					((IDisposable)crcReaderStream).Dispose();
				}
			}
		}

		private IEnumerator<IAsyncResult> PutBlockImpl(IAccountIdentifier identifier, string account, string container, string blob, byte[] blockIdentifier, Stream inputStream, long contentLength, long? contentCRC64, byte[] contentMD5, bool isLargeBlockBlobRequest, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IPutBlockResult> context)
		{
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (blockIdentifier == null || (int)blockIdentifier.Length == 0)
			{
				throw new ArgumentException("must not be null or zero-length", "blockIdentifier");
			}
			if (inputStream == null)
			{
				throw new ArgumentNullException("inputStream");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in PutBlock");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container);
			IListBlobObject listBlobObject = blobContainer.CreateBlockBlobInstance(blob);
			Stream stream = inputStream;
			CrcReaderStream crcReaderStream = null;
			MD5ReaderStream mD5ReaderStream = null;
			mD5ReaderStream = new MD5ReaderStream(stream, contentLength, false, requestContext);
			if (contentMD5 != null)
			{
				mD5ReaderStream.HashToVerifyAgainst = contentMD5;
			}
			stream = mD5ReaderStream;
			try
			{
				listBlobObject.Timeout = startingNow.Remaining(timeout);
				asyncResult = listBlobObject.BeginPutBlock(blockIdentifier, contentLength, stream, crcReaderStream, contentMD5, isLargeBlockBlobRequest, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.PutBlockImpl"));
				yield return asyncResult;
				listBlobObject.EndPutBlock(asyncResult);
				PutBlockResult hash = new PutBlockResult();
				PutBlockResult putBlockResult = hash;
				flag = (listBlobObject.IsWriteEncrypted.HasValue ? listBlobObject.IsWriteEncrypted.Value : false);
				putBlockResult.IsWriteEncrypted = flag;
				listBlobObject.Dispose();
				blobContainer.Dispose();
				try
				{
					if (!supportCRC64)
					{
						hash.ContentMD5 = mD5ReaderStream.Hash;
					}
					else
					{
						if (contentCRC64.HasValue)
						{
							hash.ContentCrc64 = new long?(crcReaderStream.CalculatedCrc);
						}
						if (contentMD5 != null)
						{
							hash.ContentMD5 = mD5ReaderStream.Hash;
						}
					}
				}
				catch (InvalidOperationException invalidOperationException1)
				{
					InvalidOperationException invalidOperationException = invalidOperationException1;
					object[] resourceString = new object[] { RealServiceManager.GetResourceString(account, container, blob) };
					NephosAssertionException.Fail(invalidOperationException, "PutBlock call for blob {0} completed successfully, however CRC64 or MD5 hash of blob contents is not available, which indicates that not all of the blob contents were read during the PutBlock call.", resourceString);
				}
				context.ResultData = hash;
			}
			finally
			{
				if (mD5ReaderStream != null)
				{
					((IDisposable)mD5ReaderStream).Dispose();
				}
				if (crcReaderStream != null)
				{
					((IDisposable)crcReaderStream).Dispose();
				}
			}
		}

		private IEnumerator<IAsyncResult> PutPageImpl(IAccountIdentifier identifier, string account, string container, string blob, Stream inputStream, BlobRegion blobRegion, long? contentCRC64, byte[] contentMD5, bool supportCRC64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<IPutPageResult> context)
		{
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (supportCRC64 && contentCRC64.HasValue && contentMD5 != null)
			{
				throw new ArgumentException("Both CRC64 and MD5 should not be provided");
			}
			if (inputStream == null)
			{
				throw new ArgumentNullException("inputStream");
			}
			if (blobRegion == null)
			{
				throw new ArgumentNullException("blobRegion");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in PutPage");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.PutPageImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IIndexBlobObject indexBlobObject = blobContainer.CreateIndexBlobObjectInstance(blob))
				{
					Stream stream = inputStream;
					CrcReaderStream crcReaderStream = null;
					MD5ReaderStream mD5ReaderStream = null;
					mD5ReaderStream = new MD5ReaderStream(stream, blobRegion.Length, false, requestContext);
					if (contentMD5 != null)
					{
						mD5ReaderStream.HashToVerifyAgainst = contentMD5;
					}
					stream = mD5ReaderStream;
					try
					{
						indexBlobObject.Timeout = startingNow.Remaining(timeout);
						asyncResult = indexBlobObject.BeginPutPage(blobRegion, stream, crcReaderStream, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.PutPageImpl"));
						yield return asyncResult;
						indexBlobObject.EndPutPage(asyncResult);
						PutPageResult value = new PutPageResult();
						DateTime? lastModificationTime = indexBlobObject.LastModificationTime;
						NephosAssertionException.Assert(lastModificationTime.HasValue, "PutPage call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
						value.LastModifiedTime = indexBlobObject.LastModificationTime.Value;
						PutPageResult putPageResult = value;
						flag = (indexBlobObject.IsWriteEncrypted.HasValue ? indexBlobObject.IsWriteEncrypted.Value : false);
						putPageResult.IsWriteEncrypted = flag;
						try
						{
							if (!supportCRC64)
							{
								value.ContentMD5 = mD5ReaderStream.Hash;
							}
							else
							{
								if (contentCRC64.HasValue)
								{
									value.ContentCrc64 = new long?(crcReaderStream.CalculatedCrc);
								}
								if (contentMD5 != null)
								{
									value.ContentMD5 = mD5ReaderStream.Hash;
								}
							}
						}
						catch (InvalidOperationException invalidOperationException1)
						{
							InvalidOperationException invalidOperationException = invalidOperationException1;
							object[] resourceString = new object[] { RealServiceManager.GetResourceString(account, container, blob) };
							NephosAssertionException.Fail(invalidOperationException, "PutPage call for blob {0} completed successfully, however CRC64 or MD5 hash of blob contents is not available, which indicates that not all of the blob contents were read during the PutPage call.", resourceString);
						}
						long? sequenceNumber = indexBlobObject.SequenceNumber;
						NephosAssertionException.Assert(sequenceNumber.HasValue, "PutPage call for blob {0} has completed successfully, however the sequence number is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
						value.SequenceNumber = indexBlobObject.SequenceNumber.Value;
						context.ResultData = value;
					}
					finally
					{
						if (mD5ReaderStream != null)
						{
							((IDisposable)mD5ReaderStream).Dispose();
						}
						if (crcReaderStream != null)
						{
							((IDisposable)crcReaderStream).Dispose();
						}
					}
				}
			}
		}

		private IEnumerator<IAsyncResult> ReleaseBlobLeaseImpl(IAccountIdentifier identifier, string account, string container, string blob, Guid leaseId, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ReleaseBlobLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ReleaseBlobLeaseImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginReleaseLease(leaseId, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.ReleaseBlobLeaseImpl"));
					yield return asyncResult;
					blobObject.EndReleaseLease(asyncResult);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "ReleaseBlobLease call for blob {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value
					};
					context.ResultData = leaseInfoResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> ReleaseContainerLeaseImpl(IAccountIdentifier identifier, string account, string container, Guid leaseId, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in ReleaseContainerLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginReleaseLease(leaseId, condition, !requestContext.IsRequestVersionAtLeastAugust13, requestContext.IsRequestVersionAtLeastMay16, context.GetResumeCallback(), context.GetResumeState("IBlobContainer.BeginReleaseLease"));
				yield return asyncResult;
				blobContainer.EndReleaseLease(asyncResult);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "ReleaseContainerLease call for container {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
				{
					LastModifiedTime = blobContainer.LastModificationTime.Value
				};
				context.ResultData = leaseInfoResult;
			}
		}

		private IEnumerator<IAsyncResult> RenewBlobLeaseImpl(IAccountIdentifier identifier, string account, string container, string blob, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in RenewBlobLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.RenewBlobLeaseImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			BlobServiceVersion blobServiceVersion = BlobServiceVersion.Original;
			if (requestContext.IsRequestVersionAtLeastFebruary12)
			{
				blobServiceVersion = BlobServiceVersion.Feb12;
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob, DateTime.MaxValue, blobServiceVersion))
				{
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginRenewLease(leaseType, leaseId, leaseDuration, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.RenewBlobLeaseImpl"));
					yield return asyncResult;
					blobObject.EndRenewLease(asyncResult);
					ILeaseInfo leaseInfo = blobObject.LeaseInfo;
					NephosAssertionException.Assert(leaseInfo != null, "RenewBlobLease call for blob {0} completed successfully, but the lease info is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					Guid? id = leaseInfo.Id;
					NephosAssertionException.Assert(id.HasValue, "RenewBlobLease call for blob {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					TimeSpan? nullable = leaseInfo.Duration;
					NephosAssertionException.Assert(nullable.HasValue, "RenewBlobLease call for blob {0} completed successfully, but the lease duration is not populated.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "RenewBlobLease call for blob {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value,
						LeaseInfo = leaseInfo
					};
					context.ResultData = leaseInfoResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> RenewContainerLeaseImpl(IAccountIdentifier identifier, string account, string container, LeaseType leaseType, Guid leaseId, TimeSpan leaseDuration, ContainerCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ILeaseInfoResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in RenewContainerLease");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("AuthorizationManager.BeginCheckAccess"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginRenewLease(leaseType, leaseId, leaseDuration, condition, !requestContext.IsRequestVersionAtLeastAugust13, requestContext.IsRequestVersionAtLeastMay16, context.GetResumeCallback(), context.GetResumeState("IBlobContainer.BeginRenewLease"));
				yield return asyncResult;
				blobContainer.EndRenewLease(asyncResult);
				ILeaseInfo leaseInfo = blobContainer.LeaseInfo;
				NephosAssertionException.Assert(leaseInfo != null, "RenewContainerLease call for container {0} completed successfully, but the lease info is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				Guid? id = leaseInfo.Id;
				NephosAssertionException.Assert(id.HasValue, "RenewContainerLease call for container {0} completed successfully, but the lease ID is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				TimeSpan? nullable = leaseInfo.Duration;
				NephosAssertionException.Assert(nullable.HasValue, "RenewContainerLease call for container {0} completed successfully, but the lease duration is not populated.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "RenewContainerLease call for container {0} has completed successfully, but the last modification time is not known.", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				LeaseInfoResult leaseInfoResult = new LeaseInfoResult()
				{
					LastModifiedTime = blobContainer.LastModificationTime.Value,
					LeaseInfo = leaseInfo
				};
				context.ResultData = leaseInfoResult;
			}
		}

		private IEnumerator<IAsyncResult> SetBlobMetadataImpl(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ISetBlobMetadataResult> context)
		{
			bool flag;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetBlobMetadata");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobMetadata"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					byte[] numArray = null;
					if (metadata != null)
					{
						try
						{
							numArray = MetadataEncoding.Encode(metadata);
						}
						catch (MetadataFormatException metadataFormatException1)
						{
							MetadataFormatException metadataFormatException = metadataFormatException1;
							throw new MetadataFormatException(string.Format("Failed to encode metadata for blob {0}: {1}", RealServiceManager.GetResourceString(account, container, blob), metadataFormatException.Message), metadataFormatException);
						}
						if ((int)numArray.Length > 8192)
						{
							int num = 8192;
							throw new XStoreArgumentOutOfRangeException("metadata", string.Concat("The metadata should be at most ", num.ToString(), " bytes."));
						}
					}
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginSetApplicationMetadata(numArray, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobMetadataImpl"));
					yield return asyncResult;
					blobObject.EndSetApplicationMetadata(asyncResult);
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "Metadata for blob {0} has been set successfully, however the last modification is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					SetBlobMetadataResult setBlobMetadataResult = new SetBlobMetadataResult()
					{
						LastModifiedTime = blobObject.LastModificationTime.Value
					};
					SetBlobMetadataResult setBlobMetadataResult1 = setBlobMetadataResult;
					flag = (blobObject.IsWriteEncrypted.HasValue ? blobObject.IsWriteEncrypted.Value : false);
					setBlobMetadataResult1.IsWriteEncrypted = flag;
					context.ResultData = setBlobMetadataResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> SetBlobPropertiesImpl(IAccountIdentifier identifier, string account, string container, string blob, IPutBlobProperties properties, bool supportCrc64, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ISetBlobPropertiesResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (properties == null)
			{
				throw new ArgumentNullException("properties");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetBlobProperties");
			}
			string str = null;
			NameValueCollection nameValueCollection = null;
			byte[] numArray = null;
			long? nullable = null;
			string resourceString = RealServiceManager.GetResourceString(account, container, blob);
			this.ConvertSetBlobProperties(resourceString, properties, supportCrc64, out nameValueCollection, out str, out nullable);
			numArray = this.EncodeServiceMetadata(resourceString, nameValueCollection);
			StorageStampHelpers.ValidateServiceProperties(numArray, str);
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.ResourceSas | SasType.AccountSas,
				SignedResourceType = SasResourceType.Object,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, blob, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobPropertiesImpl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					SetBlobPropertiesResult setBlobPropertiesResult = new SetBlobPropertiesResult();
					blobObject.Timeout = startingNow.Remaining(timeout);
					asyncResult = blobObject.BeginSetProperties(str, nullable, numArray, null, properties.SequenceNumberUpdate, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobPropertiesImpl"));
					yield return asyncResult;
					blobObject.EndSetProperties(asyncResult);
					bool hasValue = blobObject.LastModificationTime.HasValue;
					NephosAssertionException.Assert(hasValue, "SetBlobProperties call for blob {0} has completed successfully, however the last modification time is not known", new object[] { resourceString });
					setBlobPropertiesResult.LastModifiedTime = blobObject.LastModificationTime.Value;
					setBlobPropertiesResult.SequenceNumber = blobObject.SequenceNumber;
					context.ResultData = setBlobPropertiesResult;
				}
			}
		}

		private IEnumerator<IAsyncResult> SetBlobServicePropertiesImpl(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<NoResults> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(ownerAccountName))
			{
				throw new ArgumentException("ownerAccountName", "Cannot be null");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetBlobServiceProperties");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Service,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, ownerAccountName, null, null, PermissionLevel.Write | PermissionLevel.Owner, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobServiceProperties"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			IStorageAccount accountServiceMetadatum = null;
			accountServiceMetadatum = this.storageManager.CreateAccountInstance(ownerAccountName);
			accountServiceMetadatum.ServiceMetadata = new AccountServiceMetadata()
			{
				BlobAnalyticsSettings = settings
			};
			accountServiceMetadatum.Timeout = timeout;
			asyncResult = accountServiceMetadatum.BeginSetProperties(new AccountPropertyNames(AccountLevelPropertyNames.None, (AccountServiceMetadataPropertyNames)((long)32768)), null, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetBlobServiceProperties"));
			yield return asyncResult;
			accountServiceMetadatum.EndSetProperties(asyncResult);
		}

		private IEnumerator<IAsyncResult> SetContainerAclImpl(IAccountIdentifier identifier, string account, string container, ContainerAclSettings acl, DateTime? ifModifiedSince, DateTime? ifNotModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<DateTime> context)
		{
			byte[] numArray;
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (acl == null)
			{
				throw new ArgumentNullException("acl");
			}
			if (acl.SASIdentifiers == null)
			{
				throw new ArgumentNullException("sasidentifiers");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetContainerAcl");
			}
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, account, container, null, PermissionLevel.WriteAcl, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetContainerAcl"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				acl.EncodeToServiceMetadata(out numArray);
				ContainerCondition containerCondition = null;
				if (ifModifiedSince.HasValue || ifNotModifiedSince.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						IfModifiedSinceTime = ifModifiedSince,
						IfNotModifiedSinceTime = ifNotModifiedSince
					};
				}
				if (leaseId.HasValue)
				{
					if (containerCondition == null)
					{
						containerCondition = new ContainerCondition();
					}
					containerCondition.LeaseId = leaseId;
				}
				blobContainer.ServiceMetadata = numArray;
				blobContainer.Timeout = startingNow.Remaining(timeout);
				asyncResult = blobContainer.BeginSetProperties(ContainerPropertyNames.ServiceMetadata, containerCondition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetContainerAclImpl"));
				yield return asyncResult;
				blobContainer.EndSetProperties(asyncResult);
				DateTime? lastModificationTime = blobContainer.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "Acl for container {0} has been set successfully, however the last modification time is not known", new Func<string, string, string>(RealServiceManager.GetResourceString), account, container);
				context.ResultData = blobContainer.LastModificationTime.Value;
			}
		}

		private IEnumerator<IAsyncResult> SetContainerMetadataImpl(IAccountIdentifier identifier, string accountName, string containerName, NameValueCollection metadata, DateTime? ifModifiedSince, Guid? leaseId, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<DateTime> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(accountName))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(containerName))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SetContainerMetadata");
			}
			SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
			{
				SupportedSasTypes = SasType.AccountSas,
				SignedResourceType = SasResourceType.Container,
				SignedPermission = SASPermission.Write
			};
			SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
			AuthorizationInformation authorizationInformation = new AuthorizationInformation();
			IAsyncResult asyncResult = this.authorizationManager.BeginCheckAccess(identifier, accountName, containerName, null, PermissionLevel.Write, sASAuthorizationParameter1, authorizationInformation, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetContainerMetadata"));
			yield return asyncResult;
			this.authorizationManager.EndCheckAccess(asyncResult);
			using (IBaseBlobContainer operationStatus = (IBaseBlobContainer)this.storageManager.CreateContainerInstance(accountName, containerName, requestContext.ServiceType))
			{
				if (requestContext != null)
				{
					operationStatus.OperationStatus = requestContext.OperationStatus;
				}
				ContainerCondition containerCondition = null;
				if (ifModifiedSince.HasValue || leaseId.HasValue)
				{
					containerCondition = new ContainerCondition()
					{
						IfModifiedSinceTime = ifModifiedSince,
						LeaseId = leaseId
					};
				}
				byte[] numArray = null;
				if (metadata != null)
				{
					try
					{
						numArray = MetadataEncoding.Encode(metadata);
					}
					catch (MetadataFormatException metadataFormatException1)
					{
						MetadataFormatException metadataFormatException = metadataFormatException1;
						throw new MetadataFormatException(string.Format("Failed to encode metadata for container {0}: {1}", RealServiceManager.GetResourceString(accountName, containerName), metadataFormatException.Message), metadataFormatException);
					}
				}
				operationStatus.ApplicationMetadata = numArray;
				operationStatus.Timeout = startingNow.Remaining(timeout);
				asyncResult = operationStatus.BeginSetProperties(ContainerPropertyNames.ApplicationMetadata, containerCondition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SetContainerMetadataImpl"));
				yield return asyncResult;
				operationStatus.EndSetProperties(asyncResult);
				DateTime? lastModificationTime = operationStatus.LastModificationTime;
				NephosAssertionException.Assert(lastModificationTime.HasValue, "Metadata for container {0} has been set successfully, however the last modification is not known", new Func<string, string, string>(RealServiceManager.GetResourceString), accountName, containerName);
				context.ResultData = operationStatus.LastModificationTime.Value;
			}
		}

		protected static void SetServiceProperty(string resource, NameValueCollection serviceProperties, string name, string value)
		{
			if (value != null)
			{
				try
				{
					MetadataEncoding.Validate(value);
				}
				catch (FormatException formatException1)
				{
					FormatException formatException = formatException1;
					CultureInfo invariantCulture = CultureInfo.InvariantCulture;
					object[] objArray = new object[] { name, value, resource, formatException.Message };
					throw new MetadataFormatException(string.Format(invariantCulture, "Invalid {0} with value '{1}' for blob {2}: {3}", objArray), formatException);
				}
				serviceProperties.Set(name, value);
			}
		}

		private static bool ShouldRetryWithGetRequest(HttpStatusCode statusCode)
		{
			if (statusCode != HttpStatusCode.BadRequest && statusCode != HttpStatusCode.Unauthorized && statusCode != HttpStatusCode.Forbidden && statusCode != HttpStatusCode.NotFound)
			{
				return false;
			}
			return true;
		}

		private IEnumerator<IAsyncResult> SnapshotBlobImpl(IAccountIdentifier identifier, string account, string container, string blob, NameValueCollection metadata, BlobObjectCondition condition, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ISnapshotBlobResult> context)
		{
			Duration startingNow = Duration.StartingNow;
			if (identifier == null)
			{
				throw new ArgumentNullException("identifier");
			}
			if (string.IsNullOrEmpty(account))
			{
				throw new ArgumentException("account", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(container))
			{
				throw new ArgumentException("container", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(blob))
			{
				throw new ArgumentException("blob", "Cannot be null or empty");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in SnapshotBlob");
			}
			List<SASPermission> sASPermissions = new List<SASPermission>()
			{
				SASPermission.Write,
				SASPermission.Create
			};
			this.authorizationManager.CheckAccessWithMultiplePermissions(identifier, account, container, blob, PermissionLevel.Write, SasType.ResourceSas | SasType.AccountSas, SasResourceType.Object, sASPermissions, startingNow.Remaining(timeout));
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(account, container))
			{
				using (IBlobObject blobObject = blobContainer.CreateBaseBlobObjectInstance(blob))
				{
					byte[] numArray = null;
					if (metadata != null && metadata.Count > 0)
					{
						try
						{
							numArray = MetadataEncoding.Encode(metadata);
						}
						catch (MetadataFormatException metadataFormatException1)
						{
							MetadataFormatException metadataFormatException = metadataFormatException1;
							throw new MetadataFormatException(string.Format("Failed to encode metadata for blob {0}: {1}", RealServiceManager.GetResourceString(account, container, blob), metadataFormatException.Message), metadataFormatException);
						}
					}
					blobObject.Timeout = startingNow.Remaining(timeout);
					IAsyncResult asyncResult = blobObject.BeginSnapshotBlob(numArray, condition, context.GetResumeCallback(), context.GetResumeState("RealServiceManager.SnapshotBlobImpl"));
					yield return asyncResult;
					DateTime dateTime = blobObject.EndSnapshotBlob(asyncResult);
					NephosAssertionException.Assert(dateTime != DateTime.MaxValue, "The snapshot timestamp should not be the root blob timestamp!");
					DateTime? lastModificationTime = blobObject.LastModificationTime;
					NephosAssertionException.Assert(lastModificationTime.HasValue, "SnapshotBlob call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), account, container, blob);
					SnapshotBlobResult snapshotBlobResult = new SnapshotBlobResult()
					{
						SnapshotTimestamp = dateTime,
						LastModifiedTime = blobObject.LastModificationTime.Value
					};
					context.ResultData = snapshotBlobResult;
				}
			}
		}

		[SuppressMessage("Microsoft.StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification="if-def")]
		private IEnumerator<IAsyncResult> SynchronousCopyBlobImpl(IAccountIdentifier accessIdentifier, string destinationAccount, string destinationContainer, string destinationBlob, string sourceAccount, string sourceContainer, string sourceBlob, UriString copySource, bool isSourceSignedAccess, bool isDestinationSignedAccess, ICopyBlobProperties copyBlobProperties, BlobObjectCondition destinationCondition, OverwriteOption destinationOverwriteOption, BlobObjectCondition sourceCondition, DateTime? sourceSnapshot, TimeSpan timeout, RequestContext requestContext, AsyncIteratorContext<ICopyBlobResult> context)
		{
			DateTime dateTime;
			IAsyncResult asyncResult;
			Duration startingNow = Duration.StartingNow;
			CopyBlobResult elapsed = new CopyBlobResult()
			{
				CopySourceVerificationRequestRoundTripLatency = TimeSpan.Zero
			};
			context.ResultData = elapsed;
			if (accessIdentifier == null)
			{
				throw new ArgumentNullException("accessIdentifier");
			}
			if (string.IsNullOrEmpty(destinationAccount))
			{
				throw new ArgumentException("destinationAccount", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(destinationContainer))
			{
				throw new ArgumentException("destinationContainer", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(destinationBlob))
			{
				throw new ArgumentException("destinationBlob", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(sourceAccount))
			{
				throw new ArgumentException("sourceAccount", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(sourceContainer))
			{
				throw new ArgumentException("sourceContainer", "Cannot be null or empty");
			}
			if (string.IsNullOrEmpty(sourceBlob))
			{
				throw new ArgumentException("sourceBlob", "Cannot be null or empty");
			}
			if (copySource == null)
			{
				throw new ArgumentNullException("copySource");
			}
			if (timeout <= TimeSpan.Zero)
			{
				throw new TimeoutException("Timed out in CopyBlob");
			}
			byte[] numArray = null;
			if (requestContext.IsRequestVersionAtLeastApril15 || requestContext.IsRequestVersionAtLeastAugust13 && sourceAccount == destinationAccount && isDestinationSignedAccess)
			{
				List<SASPermission> sASPermissions = new List<SASPermission>()
				{
					SASPermission.Write,
					SASPermission.Create
				};
				if (this.authorizationManager.CheckAccessWithMultiplePermissions(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, PermissionLevel.Write, SasType.ResourceSas | SasType.AccountSas, SasResourceType.Object, sASPermissions, startingNow.Remaining(timeout)).SignedPermission == SASPermission.Create)
				{
					this.AuthorizationCondition = new Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager.AuthorizationCondition()
					{
						MustNotExist = true
					};
					this.ApplyAuthorizationConditions(base.AuthorizationCondition, destinationCondition, ref destinationOverwriteOption);
				}
			}
			else
			{
				asyncResult = this.authorizationManager.BeginCheckAccess(accessIdentifier, destinationAccount, destinationContainer, destinationBlob, PermissionLevel.Write, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
				yield return asyncResult;
				this.authorizationManager.EndCheckAccess(asyncResult);
			}
			bool flag = true;
			if (sourceAccount == destinationAccount && !isSourceSignedAccess && !isDestinationSignedAccess)
			{
				SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
				{
					SupportedSasTypes = SasType.ResourceSas,
					SignedResourceType = SasResourceType.Object,
					SignedPermission = SASPermission.Read
				};
				SASAuthorizationParameters sASAuthorizationParameter1 = sASAuthorizationParameter;
				asyncResult = this.authorizationManager.BeginCheckAccess(accessIdentifier, sourceAccount, sourceContainer, sourceBlob, PermissionLevel.Read, sASAuthorizationParameter1, startingNow.Remaining(timeout), context.GetResumeCallback(), context.GetResumeState("authorizationManager.BeginCheckAccess"));
				yield return asyncResult;
				this.authorizationManager.EndCheckAccess(asyncResult);
			}
			else if (flag)
			{
				Duration duration = Duration.StartingNow;
				string str = RealServiceManager.GetCopySourceRequestVersion(requestContext);
				asyncResult = RealServiceManager.BeginGetResponseFromCopySource("HEAD", copySource, str, sourceCondition, startingNow.Remaining(RealServiceManager.GetMaxTimeoutForCopySourceRequest(timeout)), context.GetResumeCallback(), context.GetResumeState("BeginGetResponseFromCopySource"));
				yield return asyncResult;
				HttpWebResponse httpWebResponse = null;
				try
				{
					httpWebResponse = RealServiceManager.EndGetResponseFromCopySource(asyncResult);
					RealServiceManager.VerifyCopySourceAccess(httpWebResponse);
				}
				finally
				{
					elapsed.CopySourceVerificationRequestRoundTripLatency = duration.Elapsed;
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] totalMilliseconds = new object[] { elapsed.CopySourceVerificationRequestRoundTripLatency.TotalMilliseconds };
					verbose.Log("FE CopyBlob: copy source verification request round trip latency: {0}ms", totalMilliseconds);
					RealServiceManager.CloseResponse(httpWebResponse);
				}
			}
			if (copyBlobProperties != null)
			{
				RealServiceManager.ConvertCopyBlobProperties(destinationAccount, destinationContainer, destinationBlob, copyBlobProperties, out numArray);
			}
			BlobServiceVersion blobServiceVersion = BlobServiceVersion.Original;
			if (requestContext.IsRequestVersionAtLeastFebruary12)
			{
				blobServiceVersion = BlobServiceVersion.Feb12;
			}
			using (IBlobContainer blobContainer = this.storageManager.CreateBlobContainerInstance(sourceAccount, sourceContainer))
			{
				using (IBlobContainer blobContainer1 = this.storageManager.CreateBlobContainerInstance(destinationAccount, destinationContainer))
				{
					IBlobContainer blobContainer2 = blobContainer;
					string str1 = sourceBlob;
					DateTime? nullable = sourceSnapshot;
					dateTime = (nullable.HasValue ? nullable.GetValueOrDefault() : DateTime.MaxValue);
					using (IBlobObject blobObject = blobContainer2.CreateBaseBlobObjectInstance(str1, dateTime, blobServiceVersion))
					{
						using (IBlobObject blobObject1 = blobContainer1.CreateBaseBlobObjectInstance(destinationBlob, StorageStampHelpers.RootBlobSnapshotVersion, blobServiceVersion))
						{
							blobObject1.Timeout = startingNow.Remaining(timeout);
							DateTime? nullable1 = null;
							asyncResult = blobObject1.BeginSynchronousCopyBlob(sourceAccount, blobObject, nullable1, numArray, destinationOverwriteOption, sourceCondition, destinationCondition, copySource, context.GetResumeCallback(), context.GetResumeState("destBlobObject.BeginSynchronousCopyBlob"));
							yield return asyncResult;
							CopyBlobOperationInfo copyBlobOperationInfo = blobObject1.EndSynchronousCopyBlob(asyncResult);
							DateTime? lastModificationTime = blobObject1.LastModificationTime;
							NephosAssertionException.Assert(lastModificationTime.HasValue, "CopyBlob call for blob {0} has completed successfully, however the last modification time is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), destinationAccount, destinationContainer, destinationBlob);
							if (requestContext.IsRequestVersionAtLeastFebruary12)
							{
								NephosAssertionException.Assert(copyBlobOperationInfo != null, "CopyBlob call for blob {0} has completed successfully, however copy info is not known", new Func<string, string, string, string>(RealServiceManager.GetResourceString), destinationAccount, destinationContainer, destinationBlob);
							}
							elapsed.LastModifiedTime = blobObject1.LastModificationTime.Value;
							elapsed.CopyInfo = copyBlobOperationInfo;
						}
					}
				}
			}
		}

		private static void VerifyCopySourceAccess(HttpWebResponse response)
		{
			if (response.StatusCode != HttpStatusCode.OK)
			{
				if (response.StatusCode < HttpStatusCode.MultipleChoices || response.StatusCode >= HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.NotModified)
				{
					throw new CannotVerifyCopySourceException(response.StatusCode, response.StatusDescription);
				}
				string str = string.Format("A redirected response (HTTP status code {0}) from the copy source is not supported.", (int)response.StatusCode);
				throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, str);
			}
			if (response.ContentLength < (long)0)
			{
				throw new CannotVerifyCopySourceException(HttpStatusCode.BadRequest, "The Content-Length for the copy source is not specified.");
			}
		}
	}
}