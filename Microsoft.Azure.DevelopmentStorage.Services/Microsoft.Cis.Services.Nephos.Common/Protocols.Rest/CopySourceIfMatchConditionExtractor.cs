using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class CopySourceIfMatchConditionExtractor : ConditionExtractor
	{
		public CopySourceIfMatchConditionExtractor()
		{
		}

		protected override bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation outputCondition)
		{
			DateTime lastModifiedTimeFromETag;
			DateTime? nullable = null;
			if (!ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				if (!string.IsNullOrEmpty(condHeaders.IfRange))
				{
					outputCondition = null;
					return false;
				}
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch) || !string.IsNullOrEmpty(condHeaders.CopySourceIfModifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (Comparison.StringContains(condHeaders.CopySourceIfMatch, ","))
				{
					throw new InvalidHeaderProtocolException("x-ms-source-if-match", condHeaders.CopySourceIfMatch);
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfUnmodifiedSince) && !HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.CopySourceIfUnmodifiedSince, out nullable))
				{
					throw new InvalidHeaderProtocolException("x-ms-source-if-unmodified-since", condHeaders.CopySourceIfUnmodifiedSince);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfModifiedSince) || !string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
				if (Comparison.StringContains(condHeaders.CopySourceIfMatch, ","))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				ConditionFailStatusCode = HttpStatusCode.PreconditionFailed
			};
			if (Comparison.StringEqualsIgnoreCase(condHeaders.CopySourceIfMatch, "*"))
			{
				outputCondition = null;
				return false;
			}
			try
			{
				lastModifiedTimeFromETag = BasicHttpProcessor.GetLastModifiedTimeFromETag(condHeaders.CopySourceIfMatch);
			}
			catch (InvalidHeaderProtocolException invalidHeaderProtocolException1)
			{
				InvalidHeaderProtocolException invalidHeaderProtocolException = invalidHeaderProtocolException1;
				if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
				{
					throw new UnrecognizedIfMatchConditionException(new bool?(true), invalidHeaderProtocolException);
				}
				throw;
			}
			if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
			{
				conditionInformation.CopySourceIfMatch = new DateTime?(lastModifiedTimeFromETag);
			}
			else
			{
				conditionInformation.CopySourceIfNotModifiedSince = new DateTime?(lastModifiedTimeFromETag);
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