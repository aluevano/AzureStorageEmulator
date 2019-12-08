using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfNoneMatchConditionExtractor : ConditionExtractor
	{
		public IfNoneMatchConditionExtractor()
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
				if (string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfMatch) || !string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (Comparison.StringContains(condHeaders.IfNoneMatch, ","))
				{
					throw new InvalidHeaderProtocolException("If-None-Match", condHeaders.IfNoneMatch);
				}
				if (!string.IsNullOrEmpty(condHeaders.IfModifiedSince) && !HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfModifiedSince, out nullable))
				{
					throw new InvalidHeaderProtocolException("If-Modified-Since", condHeaders.IfModifiedSince);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince) || !string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
				if (Comparison.StringContains(condHeaders.IfNoneMatch, ","))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation();
			if (operationType != OperationTypeForConditionParsing.ReadOperation)
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.PreconditionFailed;
			}
			else
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.NotModified;
			}
			if (!Comparison.StringEqualsIgnoreCase(condHeaders.IfNoneMatch, "*"))
			{
				DateTime lastModifiedTimeFromETag = BasicHttpProcessor.GetLastModifiedTimeFromETag(condHeaders.IfNoneMatch);
				if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
				{
					conditionInformation.IfNoneMatch = new DateTime[] { lastModifiedTimeFromETag };
				}
				else
				{
					conditionInformation.IfModifiedSince = new DateTime?(lastModifiedTimeFromETag);
				}
				if (nullable.HasValue && !HttpUtilities.DateTimesEqualsUpToSeconds(nullable.Value, lastModifiedTimeFromETag))
				{
					outputCondition = null;
					return false;
				}
			}
			else
			{
				if (operationType == OperationTypeForConditionParsing.ReadOperation)
				{
					if (!VersioningHelper.IsPreFebruary16OrInvalidVersion(requestVersion))
					{
						throw new UnsatisfiableConditionException();
					}
					outputCondition = null;
					return false;
				}
				conditionInformation.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MustNotExist);
			}
			outputCondition = conditionInformation;
			return true;
		}
	}
}