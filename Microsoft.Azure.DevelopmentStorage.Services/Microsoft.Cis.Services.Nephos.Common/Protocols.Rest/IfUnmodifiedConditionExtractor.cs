using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfUnmodifiedConditionExtractor : ConditionExtractor
	{
		public IfUnmodifiedConditionExtractor()
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
				if (string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince) || !string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfModifiedSince) || !string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.IfModifiedSince) || !string.IsNullOrEmpty(condHeaders.IfNoneMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				ConditionFailStatusCode = HttpStatusCode.PreconditionFailed
			};
			if (!HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfUnmodifiedSince, out nullable))
			{
				throw new InvalidHeaderProtocolException("If-Unmodified-Since", condHeaders.IfUnmodifiedSince);
			}
			conditionInformation.IfNotModifiedSince = new DateTime?((nullable.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable.Value.AddSeconds(1) : nullable.Value));
			conditionInformation.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MayExist);
			outputCondition = conditionInformation;
			return true;
		}
	}
}