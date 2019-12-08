using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class NephosErrorCodeStrings
	{
		public const string ResourceNameDeprecated = "ResourceNameDeprecated";

		public const string UnsupportedHttpVerb = "UnsupportedHttpVerb";

		public const string HttpsRequired = "HttpsRequired";

		public const string AccountRequiresHttps = "AccountRequiresHttps";

		public const string InternalPropertyConflict = "InternalPropertyConflict";

		public const string MissingAllInternalProperties = "MissingAllInternalProperties";

		public const string MissingContentLengthHeader = "MissingContentLengthHeader";

		public const string MissingRequiredHeader = "MissingRequiredHeader";

		public const string MissingRequiredXmlNode = "MissingRequiredXmlNode";

		public const string UnsupportedHeader = "UnsupportedHeader";

		public const string UnsupportedXmlNode = "UnsupportedXmlNode";

		public const string InvalidHeaderValue = "InvalidHeaderValue";

		public const string InvalidXmlNodeValue = "InvalidXmlNodeValue";

		public const string MissingRequiredQueryParameter = "MissingRequiredQueryParameter";

		public const string UnsupportedQueryParameter = "UnsupportedQueryParameter";

		public const string InvalidQueryParameterValue = "InvalidQueryParameterValue";

		public const string InvalidPageRange = "InvalidPageRange";

		public const string OutOfRangeXmlNodeCount = "OutOfRangeXmlNodeCount";

		public const string OutOfRangeXmlArgument = "OutOfRangeXmlArgument";

		public const string OutOfRangeQueryParameterValue = "OutOfRangeQueryParameterValue";

		public const string InvalidUri = "InvalidUri";

		public const string InvalidHttpVerb = "InvalidHttpVerb";

		public const string EmptyMetadataKey = "EmptyMetadataKey";

		public const string RequestBodyTooLarge = "RequestBodyTooLarge";

		public const string InvalidXmlDocument = "InvalidXmlDocument";

		public const string InternalError = "InternalError";

		public const string AuthenticationFailed = "AuthenticationFailed";

		public const string InvalidAuthenticationInfo = "InvalidAuthenticationInfo";

		public const string AccessDenied = "AccessDenied";

		public const string BothCrc64AndMd5HeaderPresent = "BothCrc64AndMd5HeaderPresent";

		public const string BothCrc64AndMsMd5HeaderPresent = "BothCrc64AndMsMd5HeaderPresent";

		public const string BothCrc64AndMd5RangeHeaderPresent = "BothCrc64AndMd5RangeHeaderPresent";

		public const string ComputeMd5HeaderUsedWithOtherCrc64Md5Header = "ComputeMd5HeaderUsedWithOtherCrc64Md5Header";

		public const string Crc64Mismatch = "Crc64Mismatch";

		public const string InvalidCrc64 = "InvalidCrc64";

		public const string Md5Mismatch = "Md5Mismatch";

		public const string InvalidMd5 = "InvalidMd5";

		public const string OutOfRangeInput = "OutOfRangeInput";

		public const string InvalidInput = "InvalidInput";

		public const string OperationTimedOut = "OperationTimedOut";

		public const string ResourceNotFound = "ResourceNotFound";

		public const string ResourceAlreadyExists = "ResourceAlreadyExists";

		public const string ResourceTypeMismatch = "ResourceTypeMismatch";

		public const string InvalidResourceName = "InvalidResourceName";

		public const string InvalidMetadata = "InvalidMetadata";

		public const string MetadataTooLarge = "MetadataTooLarge";

		public const string MultipleVersionsSpecified = "MultipleVersionsSpecified";

		public const string ConditionNotMet = "ConditionNotMet";

		public const string UnsatisfiableCondition = "UnsatisfiableCondition";

		public const string SourceConditionNotMet = "SourceConditionNotMet";

		public const string TargetConditionNotMet = "TargetConditionNotMet";

		public const string SequenceNumberConditionNotMet = "SequenceNumberConditionNotMet";

		public const string InvalidRange = "InvalidRange";

		public const string ContainerNotFound = "ContainerNotFound";

		public const string ContainerAlreadyExists = "ContainerAlreadyExists";

		public const string CorsPreflightFailure = "CorsPreflightFailure";

		public const string ShareAlreadyExists = "ShareAlreadyExists";

		public const string ShareNotFound = "ShareNotFound";

		public const string FeatureVersionMismatch = "FeatureVersionMismatch";

		public const string InvalidOperationOnRootContainer = "InvalidOperationOnRootContainer";

		public const string ServerBusy = "ServerBusy";

		public const string HostInformationNotPresent = "HostInformationNotPresent";

		public const string RequestUrlFailedToParse = "RequestUrlFailedToParse";

		public const string UnsupportedHttpVersion = "UnsupportedHttpVersion";

		public const string MultipleConditionHeadersNotSupported = "MultipleConditionHeadersNotSupported";

		public const string ConditionHeadersNotSupported = "ConditionHeadersNotSupported";

		public const string NotImplemented = "NotImplemented";

		public const string AuthorizationSourceIPMismatch = "AuthorizationSourceIPMismatch";

		public const string AuthorizationProtocolMismatch = "AuthorizationProtocolMismatch";

		public const string AuthorizationServiceMismatch = "AuthorizationServiceMismatch";

		public const string AuthorizationPermissionMismatch = "AuthorizationPermissionMismatch";

		public const string AuthorizationResourceTypeMismatch = "AuthorizationResourceTypeMismatch";

		public const string AuthorizationFailure = "AuthorizationFailure";

		public const string InvalidOperation = "InvalidOperation";

		public const string BlobTierInadequateForContentLength = "BlobTierInadequateForContentLength";

		public const string CannotChangeToLowerTier = "CannotChangeToLowerTier";

		public const string InvalidBlobTier = "InvalidBlobTier";

		public const string ContentLengthLargerThanTierLimit = "ContentLengthLargerThanTierLimit";

		public const string FeatureNotSupportedOnStorageAccount = "FeatureNotSupportedOnStorageAccount";
	}
}