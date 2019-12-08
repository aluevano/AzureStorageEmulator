using System;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class CommonStatusEntries
	{
		public readonly static NephosStatusEntry DeprecatedResourceName;

		public readonly static NephosStatusEntry UnsupportedHttpVerb;

		public readonly static NephosStatusEntry MissingContentLengthHeader;

		public readonly static NephosStatusEntry MissingRequiredHeader;

		public readonly static NephosStatusEntry MissingAllInternalProperties;

		public readonly static NephosStatusEntry InternalPropertyConflict;

		public readonly static NephosStatusEntry MissingRequiredXmlNode;

		public readonly static NephosStatusEntry UnsupportedHeader;

		public readonly static NephosStatusEntry UnsupportedXmlNode;

		public readonly static NephosStatusEntry InvalidHeaderValue;

		public readonly static NephosStatusEntry InvalidXmlNodeValue;

		public readonly static NephosStatusEntry MissingRequiredQueryParameter;

		public readonly static NephosStatusEntry UnsupportedQueryParameter;

		public readonly static NephosStatusEntry InvalidQueryParameterValue;

		public readonly static NephosStatusEntry OutOfRangeXmlNodeCount;

		public readonly static NephosStatusEntry OutOfRangeXmlArgument;

		public readonly static NephosStatusEntry OutOfRangeQueryParameterValue;

		public readonly static NephosStatusEntry InvalidUri;

		public readonly static NephosStatusEntry InvalidHttpVerb;

		public readonly static NephosStatusEntry UnsupportedHttpVersion;

		public readonly static NephosStatusEntry EmptyMetadataKey;

		public readonly static NephosStatusEntry RequestBodyTooLarge;

		public readonly static NephosStatusEntry InvalidXmlDocument;

		public readonly static NephosStatusEntry InternalError;

		public readonly static NephosStatusEntry AuthenticationFailed;

		public readonly static NephosStatusEntry CertificateAuthenticationFailed;

		public readonly static NephosStatusEntry InvalidAuthenticationInfo;

		public readonly static NephosStatusEntry ResourceNotFound;

		public readonly static NephosStatusEntry ResourceAlreadyExists;

		public readonly static NephosStatusEntry ResourceTypeMismatch;

		public readonly static NephosStatusEntry InvalidResourceName;

		public readonly static NephosStatusEntry HttpsRequired;

		public readonly static NephosStatusEntry AccountRequiresHttps;

		public readonly static NephosStatusEntry UnAuthorized;

		public readonly static NephosStatusEntry AccessDenied;

		public readonly static NephosStatusEntry AuthorizationSourceIPMismatch;

		public readonly static NephosStatusEntry AuthorizationProtocolMismatch;

		public readonly static NephosStatusEntry AuthorizationServiceMismatch;

		public readonly static NephosStatusEntry AuthorizationPermissionMismatch;

		public readonly static NephosStatusEntry AuthorizationResourceTypeMismatch;

		public readonly static NephosStatusEntry AuthorizationFailure;

		public readonly static NephosStatusEntry BothCrc64AndMd5HeaderPresent;

		public readonly static NephosStatusEntry BothCrc64AndMsMd5HeaderPresent;

		public readonly static NephosStatusEntry BothCrc64AndMd5RangeHeaderPresent;

		public readonly static NephosStatusEntry ComputeMd5HeaderUsedWithOtherCrc64Md5Header;

		public readonly static NephosStatusEntry Crc64Mismatch;

		public readonly static NephosStatusEntry InvalidCrc64;

		public readonly static NephosStatusEntry Md5Mismatch;

		public readonly static NephosStatusEntry InvalidMd5;

		public readonly static NephosStatusEntry OutOfRangeInput;

		public readonly static NephosStatusEntry InvalidInput;

		public readonly static NephosStatusEntry OperationTimedOut;

		public readonly static NephosStatusEntry InvalidMetadata;

		public readonly static NephosStatusEntry MetadataTooLarge;

		public readonly static NephosStatusEntry MultipleVersionsSpecified;

		public readonly static NephosStatusEntry DefaultConditionNotMet;

		public readonly static NephosStatusEntry SourceConditionNotMet;

		public readonly static NephosStatusEntry TargetConditionNotMet;

		public readonly static NephosStatusEntry ConditionNotMetWithNotModifiedStatusCode;

		public readonly static NephosStatusEntry SequenceNumberConditionNotMet;

		public readonly static NephosStatusEntry InvalidRange;

		public readonly static NephosStatusEntry ContainerNotFound;

		public readonly static NephosStatusEntry ShareNotFound;

		public readonly static NephosStatusEntry ContainerAlreadyExists;

		public readonly static NephosStatusEntry ShareAlreadyExists;

		public readonly static NephosStatusEntry InvalidOperationOnRootContainer;

		public readonly static NephosStatusEntry BlobNotFound;

		public readonly static NephosStatusEntry ServerBusy;

		public readonly static NephosStatusEntry IngressThrottle;

		public readonly static NephosStatusEntry EgressThrottle;

		public readonly static NephosStatusEntry TPSThrottle;

		public readonly static NephosStatusEntry HostInformationNotPresent;

		public readonly static NephosStatusEntry RequestUrlFailedToParse;

		public readonly static NephosStatusEntry SecondaryWriteNotAllowed;

		public readonly static NephosStatusEntry SecondaryReadDisabled;

		public readonly static NephosStatusEntry AccountNotFound;

		public readonly static NephosStatusEntry MultipleConditionHeadersNotSupported;

		public readonly static NephosStatusEntry ConditionHeadersNotSupported;

		public readonly static NephosStatusEntry UnsatisfiableCondition;

		public readonly static NephosStatusEntry NotImplemented;

		public readonly static NephosStatusEntry CorsFailure;

		public readonly static NephosStatusEntry CorsRequiredHeader;

		public readonly static NephosStatusEntry UnsupportedPermissionInStoredAccessPolicy;

		static CommonStatusEntries()
		{
			CommonStatusEntries.DeprecatedResourceName = new NephosStatusEntry("ResourceNameDeprecated", HttpStatusCode.BadRequest, "The resource name specified has been deprecated for the specified request version.");
			CommonStatusEntries.UnsupportedHttpVerb = new NephosStatusEntry("UnsupportedHttpVerb", HttpStatusCode.MethodNotAllowed, "The resource doesn't support specified Http Verb.");
			CommonStatusEntries.MissingContentLengthHeader = new NephosStatusEntry("MissingContentLengthHeader", HttpStatusCode.LengthRequired, "Content-Length HTTP header is missing.");
			CommonStatusEntries.MissingRequiredHeader = new NephosStatusEntry("MissingRequiredHeader", HttpStatusCode.BadRequest, "An HTTP header that's mandatory for this request is not specified.");
			CommonStatusEntries.MissingAllInternalProperties = new NephosStatusEntry("MissingAllInternalProperties", HttpStatusCode.BadRequest, "At least one internal property should be provided for this operation.");
			CommonStatusEntries.InternalPropertyConflict = new NephosStatusEntry("InternalPropertyConflict", HttpStatusCode.BadRequest, "The request specified headers for both setting and resetting an internal property.");
			CommonStatusEntries.MissingRequiredXmlNode = new NephosStatusEntry("MissingRequiredXmlNode", HttpStatusCode.BadRequest, "An XML node that's mandatory for this request is not specified.");
			CommonStatusEntries.UnsupportedHeader = new NephosStatusEntry("UnsupportedHeader", HttpStatusCode.BadRequest, "One of the HTTP headers specified in the request is not supported.");
			CommonStatusEntries.UnsupportedXmlNode = new NephosStatusEntry("UnsupportedXmlNode", HttpStatusCode.BadRequest, "One of the XML nodes specified in the request is not supported.");
			CommonStatusEntries.InvalidHeaderValue = new NephosStatusEntry("InvalidHeaderValue", HttpStatusCode.BadRequest, "The value for one of the HTTP headers is not in the correct format.");
			CommonStatusEntries.InvalidXmlNodeValue = new NephosStatusEntry("InvalidXmlNodeValue", HttpStatusCode.BadRequest, "The value for one of the XML nodes is not in the correct format.");
			CommonStatusEntries.MissingRequiredQueryParameter = new NephosStatusEntry("MissingRequiredQueryParameter", HttpStatusCode.BadRequest, "A query parameter that's mandatory for this request is not specified.");
			CommonStatusEntries.UnsupportedQueryParameter = new NephosStatusEntry("UnsupportedQueryParameter", HttpStatusCode.BadRequest, "One of the query parameters specified in the request URI is not supported.");
			CommonStatusEntries.InvalidQueryParameterValue = new NephosStatusEntry("InvalidQueryParameterValue", HttpStatusCode.BadRequest, "Value for one of the query parameters specified in the request URI is invalid.");
			CommonStatusEntries.OutOfRangeXmlNodeCount = new NephosStatusEntry("OutOfRangeXmlNodeCount", HttpStatusCode.BadRequest, "The count of the XML nodes contained in the request body is outside the permissible range.");
			CommonStatusEntries.OutOfRangeXmlArgument = new NephosStatusEntry("OutOfRangeXmlArgument", HttpStatusCode.BadRequest, "The XML argument specified in the request body is outside the permissible range.");
			CommonStatusEntries.OutOfRangeQueryParameterValue = new NephosStatusEntry("OutOfRangeQueryParameterValue", HttpStatusCode.BadRequest, "One of the query parameters specified in the request URI is outside the permissible range.");
			CommonStatusEntries.InvalidUri = new NephosStatusEntry("InvalidUri", HttpStatusCode.BadRequest, "The requested URI does not represent any resource on the server.");
			CommonStatusEntries.InvalidHttpVerb = new NephosStatusEntry("InvalidHttpVerb", HttpStatusCode.BadRequest, "The HTTP verb specified is invalid - it is not recognized by the server.");
			CommonStatusEntries.UnsupportedHttpVersion = new NephosStatusEntry("UnsupportedHttpVersion", HttpStatusCode.HttpVersionNotSupported, "The HTTP version specified is not supported for this operation by the server.");
			CommonStatusEntries.EmptyMetadataKey = new NephosStatusEntry("EmptyMetadataKey", HttpStatusCode.BadRequest, "The key for one of the metadata key-value pairs is empty.");
			CommonStatusEntries.RequestBodyTooLarge = new NephosStatusEntry("RequestBodyTooLarge", HttpStatusCode.RequestEntityTooLarge, "The request body is too large and exceeds the maximum permissible limit.");
			CommonStatusEntries.InvalidXmlDocument = new NephosStatusEntry("InvalidXmlDocument", HttpStatusCode.BadRequest, "XML specified is not syntactically valid.");
			CommonStatusEntries.InternalError = new NephosStatusEntry("InternalError", HttpStatusCode.InternalServerError, "Server encountered an internal error. Please try again after some time.");
			CommonStatusEntries.AuthenticationFailed = new NephosStatusEntry("AuthenticationFailed", HttpStatusCode.Forbidden, "Server failed to authenticate the request. Make sure the value of Authorization header is formed correctly including the signature.");
			CommonStatusEntries.CertificateAuthenticationFailed = new NephosStatusEntry("AuthenticationFailed", HttpStatusCode.Forbidden, "Server failed to authenticate the request.");
			CommonStatusEntries.InvalidAuthenticationInfo = new NephosStatusEntry("InvalidAuthenticationInfo", HttpStatusCode.BadRequest, "Authentication information is not given in the correct format. Check the value of Authorization header.");
			CommonStatusEntries.ResourceNotFound = new NephosStatusEntry("ResourceNotFound", HttpStatusCode.NotFound, "The specified resource does not exist.");
			CommonStatusEntries.ResourceAlreadyExists = new NephosStatusEntry("ResourceAlreadyExists", HttpStatusCode.Conflict, "The specified resource already exists.");
			CommonStatusEntries.ResourceTypeMismatch = new NephosStatusEntry("ResourceTypeMismatch", HttpStatusCode.Conflict, "The specified resource type does not match the type of the existing resource.");
			CommonStatusEntries.InvalidResourceName = new NephosStatusEntry("InvalidResourceName", HttpStatusCode.BadRequest, "The specifed resource name contains invalid characters.");
			CommonStatusEntries.HttpsRequired = new NephosStatusEntry("HttpsRequired", HttpStatusCode.BadRequest, "The request should be sent using https.");
			CommonStatusEntries.AccountRequiresHttps = new NephosStatusEntry("AccountRequiresHttps", HttpStatusCode.BadRequest, "The account being accessed does not support http.");
			CommonStatusEntries.UnAuthorized = CommonStatusEntries.ResourceNotFound;
			CommonStatusEntries.AccessDenied = new NephosStatusEntry("AccessDenied", HttpStatusCode.Forbidden, "Access to this account is denied.");
			CommonStatusEntries.AuthorizationSourceIPMismatch = new NephosStatusEntry("AuthorizationSourceIPMismatch", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation using this source IP {0}.");
			CommonStatusEntries.AuthorizationProtocolMismatch = new NephosStatusEntry("AuthorizationProtocolMismatch", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation using this protocol.");
			CommonStatusEntries.AuthorizationServiceMismatch = new NephosStatusEntry("AuthorizationServiceMismatch", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation using this service.");
			CommonStatusEntries.AuthorizationPermissionMismatch = new NephosStatusEntry("AuthorizationPermissionMismatch", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation using this permission.");
			CommonStatusEntries.AuthorizationResourceTypeMismatch = new NephosStatusEntry("AuthorizationResourceTypeMismatch", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation using this resource type.");
			CommonStatusEntries.AuthorizationFailure = new NephosStatusEntry("AuthorizationFailure", HttpStatusCode.Forbidden, "This request is not authorized to perform this operation.");
			CommonStatusEntries.BothCrc64AndMd5HeaderPresent = new NephosStatusEntry("BothCrc64AndMd5HeaderPresent", HttpStatusCode.BadRequest, "Both x-ms-content-crc64 header and Content-MD5 header are present.");
			CommonStatusEntries.BothCrc64AndMsMd5HeaderPresent = new NephosStatusEntry("BothCrc64AndMsMd5HeaderPresent", HttpStatusCode.BadRequest, "Both x-ms-content-crc64 header and x-ms-blob-content-md5 header are present.");
			CommonStatusEntries.BothCrc64AndMd5RangeHeaderPresent = new NephosStatusEntry("BothCrc64AndMd5RangeHeaderPresent", HttpStatusCode.BadRequest, "Both x-ms-range-get-content-crc64 header and x-ms-range-get-content-md5 header are present.");
			CommonStatusEntries.ComputeMd5HeaderUsedWithOtherCrc64Md5Header = new NephosStatusEntry("ComputeMd5HeaderUsedWithOtherCrc64Md5Header", HttpStatusCode.BadRequest, "x-ms-put-blob-compute-md5 header cannot be used in conjunction with other CRC64 or MD5 headers.");
			CommonStatusEntries.Crc64Mismatch = new NephosStatusEntry("Crc64Mismatch", HttpStatusCode.BadRequest, "The CRC64 value specified in the request did not match with the CRC64 value calculated by the server.");
			CommonStatusEntries.InvalidCrc64 = new NephosStatusEntry("InvalidCrc64", HttpStatusCode.BadRequest, "The CRC64 value specified in the request is invalid. CRC64 value must be 64 bits and base64 encoded.");
			CommonStatusEntries.Md5Mismatch = new NephosStatusEntry("Md5Mismatch", HttpStatusCode.BadRequest, "The MD5 value specified in the request did not match with the MD5 value calculated by the server.");
			CommonStatusEntries.InvalidMd5 = new NephosStatusEntry("InvalidMd5", HttpStatusCode.BadRequest, "The MD5 value specified in the request is invalid. MD5 value must be 128 bits and base64 encoded.");
			CommonStatusEntries.OutOfRangeInput = new NephosStatusEntry("OutOfRangeInput", HttpStatusCode.BadRequest, "One of the request inputs is out of range.");
			CommonStatusEntries.InvalidInput = new NephosStatusEntry("InvalidInput", HttpStatusCode.BadRequest, "One of the request inputs is not valid.");
			CommonStatusEntries.OperationTimedOut = new NephosStatusEntry("OperationTimedOut", HttpStatusCode.InternalServerError, "Operation could not be completed within the specified time.");
			CommonStatusEntries.InvalidMetadata = new NephosStatusEntry("InvalidMetadata", HttpStatusCode.BadRequest, "The metadata specified is invalid. It has characters that are not permitted.");
			CommonStatusEntries.MetadataTooLarge = new NephosStatusEntry("MetadataTooLarge", HttpStatusCode.BadRequest, "The metadata specified exceeds the maximum permissible limit (8KB).");
			CommonStatusEntries.MultipleVersionsSpecified = new NephosStatusEntry("MultipleVersionsSpecified", HttpStatusCode.BadRequest, "Request can specify either x-ms-version header or api-version query parameter but not both.");
			CommonStatusEntries.DefaultConditionNotMet = new NephosStatusEntry("ConditionNotMet", HttpStatusCode.PreconditionFailed, "The condition specified using HTTP conditional header(s) is not met.");
			CommonStatusEntries.SourceConditionNotMet = new NephosStatusEntry("SourceConditionNotMet", HttpStatusCode.PreconditionFailed, "The source condition specified using HTTP conditional header(s) is not met.");
			CommonStatusEntries.TargetConditionNotMet = new NephosStatusEntry("TargetConditionNotMet", HttpStatusCode.PreconditionFailed, "The target condition specified using HTTP conditional header(s) is not met.");
			CommonStatusEntries.ConditionNotMetWithNotModifiedStatusCode = new NephosStatusEntry(CommonStatusEntries.DefaultConditionNotMet.StatusId, HttpStatusCode.NotModified, CommonStatusEntries.DefaultConditionNotMet.UserMessage);
			CommonStatusEntries.SequenceNumberConditionNotMet = new NephosStatusEntry("SequenceNumberConditionNotMet", HttpStatusCode.PreconditionFailed, "The sequence number condition specified was not met.");
			CommonStatusEntries.InvalidRange = new NephosStatusEntry("InvalidRange", HttpStatusCode.RequestedRangeNotSatisfiable, "The range specified is invalid for the current size of the resource.");
			CommonStatusEntries.ContainerNotFound = new NephosStatusEntry("ContainerNotFound", HttpStatusCode.NotFound, "The specified container does not exist.");
			CommonStatusEntries.ShareNotFound = new NephosStatusEntry("ShareNotFound", HttpStatusCode.NotFound, "The specified share does not exist.");
			CommonStatusEntries.ContainerAlreadyExists = new NephosStatusEntry("ContainerAlreadyExists", HttpStatusCode.Conflict, "The specified container already exists.");
			CommonStatusEntries.ShareAlreadyExists = new NephosStatusEntry("ShareAlreadyExists", HttpStatusCode.Conflict, "The specified share already exists.");
			CommonStatusEntries.InvalidOperationOnRootContainer = new NephosStatusEntry("InvalidOperationOnRootContainer", HttpStatusCode.BadRequest, "The requested operation is not allowed on the root container.");
			CommonStatusEntries.BlobNotFound = new NephosStatusEntry("BlobNotFound", HttpStatusCode.NotFound, "The specified blob does not exist.");
			CommonStatusEntries.ServerBusy = new NephosStatusEntry("ServerBusy", HttpStatusCode.ServiceUnavailable, "The server is busy.");
			CommonStatusEntries.IngressThrottle = new NephosStatusEntry("ServerBusy", HttpStatusCode.ServiceUnavailable, "Ingress is over the account limit.");
			CommonStatusEntries.EgressThrottle = new NephosStatusEntry("ServerBusy", HttpStatusCode.ServiceUnavailable, "Egress is over the account limit.");
			CommonStatusEntries.TPSThrottle = new NephosStatusEntry("ServerBusy", HttpStatusCode.ServiceUnavailable, "Operations per second is over the account limit.");
			CommonStatusEntries.HostInformationNotPresent = new NephosStatusEntry("HostInformationNotPresent", HttpStatusCode.BadRequest, "The Host information is not present in the request. You must send a non-empty Host header or include the absolute URI in the request line.");
			CommonStatusEntries.RequestUrlFailedToParse = new NephosStatusEntry("RequestUrlFailedToParse", HttpStatusCode.BadRequest, "The url in the request could not be parsed.");
			CommonStatusEntries.SecondaryWriteNotAllowed = new NephosStatusEntry("InsufficientAccountPermissions", HttpStatusCode.Forbidden, "Write operations are not allowed.");
			CommonStatusEntries.SecondaryReadDisabled = new NephosStatusEntry("InsufficientAccountPermissions", HttpStatusCode.Forbidden, "Read operations are currently disabled.");
			CommonStatusEntries.AccountNotFound = new NephosStatusEntry("AccountNotFound", HttpStatusCode.NotFound, "The specified account does not exist.");
			CommonStatusEntries.MultipleConditionHeadersNotSupported = new NephosStatusEntry("MultipleConditionHeadersNotSupported", HttpStatusCode.BadRequest, "Multiple condition headers are not supported.");
			CommonStatusEntries.ConditionHeadersNotSupported = new NephosStatusEntry("ConditionHeadersNotSupported", HttpStatusCode.BadRequest, "Condition headers are not supported.");
			CommonStatusEntries.UnsatisfiableCondition = new NephosStatusEntry("UnsatisfiableCondition", HttpStatusCode.BadRequest, "The request includes an unsatisfiable condition for this operation.");
			CommonStatusEntries.NotImplemented = new NephosStatusEntry("NotImplemented", HttpStatusCode.NotImplemented, "The requested operation is not implemented on the specified resource.");
			CommonStatusEntries.CorsFailure = new NephosStatusEntry("CorsPreflightFailure", HttpStatusCode.Forbidden, "CORS not enabled or no matching rule found for this request.");
			CommonStatusEntries.CorsRequiredHeader = new NephosStatusEntry("InvalidHeaderValue", HttpStatusCode.BadRequest, "A required CORS header is not present.");
			CommonStatusEntries.UnsupportedPermissionInStoredAccessPolicy = new NephosStatusEntry("FeatureVersionMismatch", HttpStatusCode.Conflict, "Stored access policy contains a permission that is not supported by this version.");
		}
	}
}