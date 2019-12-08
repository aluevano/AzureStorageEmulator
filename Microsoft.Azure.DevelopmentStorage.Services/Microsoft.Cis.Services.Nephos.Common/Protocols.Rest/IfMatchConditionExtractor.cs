using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfMatchConditionExtractor : ConditionExtractor
	{
		public IfMatchConditionExtractor()
		{
		}

		protected override bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation outputCondition)
		{
			DateTime lastModifiedTimeFromETag;
			bool? nullable;
			DateTime? nullable1 = null;
			if (!ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				if (!string.IsNullOrEmpty(condHeaders.IfRange))
				{
					outputCondition = null;
					return false;
				}
				if (string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfNoneMatch) || !string.IsNullOrEmpty(condHeaders.IfModifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (Comparison.StringContains(condHeaders.IfMatch, ","))
				{
					throw new InvalidHeaderProtocolException("If-Match", condHeaders.IfMatch);
				}
				if (!string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince) && !HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfUnmodifiedSince, out nullable1))
				{
					throw new InvalidHeaderProtocolException("If-Unmodified-Since", condHeaders.IfUnmodifiedSince);
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfModifiedSince) || !string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
				if (Comparison.StringContains(condHeaders.IfMatch, ","))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				ConditionFailStatusCode = HttpStatusCode.PreconditionFailed,
				ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MustExist)
			};
			if (!Comparison.StringEqualsIgnoreCase(condHeaders.IfMatch, "*"))
			{
				try
				{
					lastModifiedTimeFromETag = BasicHttpProcessor.GetLastModifiedTimeFromETag(condHeaders.IfMatch);
				}
				catch (InvalidHeaderProtocolException invalidHeaderProtocolException1)
				{
					InvalidHeaderProtocolException invalidHeaderProtocolException = invalidHeaderProtocolException1;
					if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
					{
						if (operationType == OperationTypeForConditionParsing.CopyOperation)
						{
							nullable = new bool?(false);
						}
						else
						{
							nullable = null;
						}
						throw new UnrecognizedIfMatchConditionException(nullable, invalidHeaderProtocolException);
					}
					throw;
				}
				if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
				{
					conditionInformation.IfMatch = new DateTime[] { lastModifiedTimeFromETag };
				}
				else
				{
					conditionInformation.IfNotModifiedSince = new DateTime?(lastModifiedTimeFromETag);
					conditionInformation.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MayExist);
				}
				if (nullable1.HasValue && !HttpUtilities.DateTimesEqualsUpToSeconds(nullable1.Value, lastModifiedTimeFromETag))
				{
					outputCondition = null;
					return false;
				}
			}
			else
			{
				if (operationType == OperationTypeForConditionParsing.ReadOperation)
				{
					outputCondition = null;
					return false;
				}
				if (nullable1.HasValue)
				{
					conditionInformation.IfNotModifiedSince = new DateTime?((nullable1.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable1.Value.AddSeconds(1) : nullable1.Value));
				}
			}
			outputCondition = conditionInformation;
			return true;
		}
	}
}