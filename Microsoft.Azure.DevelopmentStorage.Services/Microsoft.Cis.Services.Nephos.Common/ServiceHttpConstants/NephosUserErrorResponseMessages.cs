using System;

namespace Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants
{
	public static class NephosUserErrorResponseMessages
	{
		public const string IPAclInvalidAllowListMessage = "AllowList and DisallowList should not be specified together.";

		public const string IPAclInvalidIPRangeMessage = "The specified starting IP address must not exceed the specified ending IP address.";

		public const string IPAclInvalidOverlappingRangesMessage = "The specified IP Ranges must not contain overlapping ranges.";

		public const string IPAclIPRangesRequiredMessage = "IPv4RangeList must be specified with Scope: List.";

		public const string IPAclMaxIPRangesExceededMessage = "The number of IP Ranges specified must not exceed the maximum number of allowed IP Ranges.";

		public const string NameLengthOutOfRange = "The specified resource name length is not within the permissible limits.";

		public const string CopySourceRequestBodyTooLarge = "The source request body is too large and exceeds the maximum permissible limit.";

		public const string DefaultInvalidInputResponseMessage = "One of the request inputs is not valid.";

		public const string DefaultArgumentOutOfRangeResponseMessage = "One of the request inputs is out of range.";

		public const string DefaultThrottlingMessage = "The server is busy.";

		public const string IngressThrottlingMessage = "Ingress is over the account limit.";

		public const string EgressThrottlingMessage = "Egress is over the account limit.";

		public const string TPSThrottlingMessage = "Operations per second is over the account limit.";
	}
}