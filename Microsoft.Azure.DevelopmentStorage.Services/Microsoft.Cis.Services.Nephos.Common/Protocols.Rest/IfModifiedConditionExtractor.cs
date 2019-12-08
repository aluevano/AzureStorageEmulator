using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfModifiedConditionExtractor : ConditionExtractor
	{
		public IfModifiedConditionExtractor()
		{
		}

		protected override bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation outputCondition)
		{
			DateTime? nullable;
			if (!ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				if (!string.IsNullOrEmpty(condHeaders.IfRange))
				{
					outputCondition = null;
					return false;
				}
				if (string.IsNullOrEmpty(condHeaders.IfModifiedSince) || !string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfMatch) || !string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince))
				{
					outputCondition = null;
					return false;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.IfModifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince) || !string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation();
			if (operationType == OperationTypeForConditionParsing.CopyOperation)
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.PreconditionFailed;
			}
			else if (operationType != OperationTypeForConditionParsing.WriteOperation)
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.NotModified;
			}
			else if (!VersioningHelper.IsPreSeptember09OrInvalidVersion(requestVersion))
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.PreconditionFailed;
			}
			else
			{
				conditionInformation.ConditionFailStatusCode = HttpStatusCode.NotModified;
			}
			if (!HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfModifiedSince, out nullable))
			{
				throw new InvalidHeaderProtocolException("If-Modified-Since", condHeaders.IfModifiedSince);
			}
			conditionInformation.IfModifiedSince = new DateTime?((nullable.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable.Value.AddSeconds(1) : nullable.Value));
			conditionInformation.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MayExist);
			outputCondition = conditionInformation;
			return true;
		}
	}
}