using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class CopySourceIfNoneMatchConditionExtractor : ConditionExtractor
	{
		public CopySourceIfNoneMatchConditionExtractor()
		{
		}

		protected override bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation outputCondition)
		{
			DateTime? nullable = null;
			if (!ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				if (!string.IsNullOrEmpty(condHeaders.IfRange))
				{
					outputCondition = null;
					return false;
				}
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfMatch) || !string.IsNullOrEmpty(condHeaders.CopySourceIfUnmodifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (Comparison.StringContains(condHeaders.CopySourceIfNoneMatch, ","))
				{
					throw new InvalidHeaderProtocolException("x-ms-source-if-none-match", condHeaders.CopySourceIfNoneMatch);
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfModifiedSince) && !HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.CopySourceIfModifiedSince, out nullable))
				{
					throw new InvalidHeaderProtocolException("x-ms-source-if-modified-since", condHeaders.CopySourceIfModifiedSince);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfUnmodifiedSince) || !string.IsNullOrEmpty(condHeaders.CopySourceIfMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
				if (Comparison.StringContains(condHeaders.CopySourceIfNoneMatch, ","))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				ConditionFailStatusCode = HttpStatusCode.PreconditionFailed
			};
			if (Comparison.StringEqualsIgnoreCase(condHeaders.CopySourceIfNoneMatch, "*"))
			{
				outputCondition = null;
				return false;
			}
			DateTime lastModifiedTimeFromETag = BasicHttpProcessor.GetLastModifiedTimeFromETag(condHeaders.CopySourceIfNoneMatch);
			if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
			{
				conditionInformation.CopySourceIfNoneMatch = new DateTime?(lastModifiedTimeFromETag);
			}
			else
			{
				conditionInformation.CopySourceIfModifiedSince = new DateTime?(lastModifiedTimeFromETag);
			}
			if (nullable.HasValue && !HttpUtilities.DateTimesEqualsUpToSeconds(nullable.Value, lastModifiedTimeFromETag))
			{
				outputCondition = null;
				return false;
			}
			outputCondition = conditionInformation;
			return true;
		}
	}
}