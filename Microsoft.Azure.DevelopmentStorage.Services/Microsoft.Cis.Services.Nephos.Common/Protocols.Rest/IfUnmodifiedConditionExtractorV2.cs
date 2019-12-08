using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfUnmodifiedConditionExtractorV2 : IConditionExtractor
	{
		public IfUnmodifiedConditionExtractorV2()
		{
		}

		public bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation outputCondition)
		{
			DateTime? nullable;
			if (!string.IsNullOrEmpty(condHeaders.IfRange))
			{
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfUnmodifiedSince))
			{
				return false;
			}
			outputCondition.ConditionFailStatusCode = HttpStatusCode.PreconditionFailed;
			if (!HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfUnmodifiedSince, out nullable))
			{
				throw new InvalidHeaderProtocolException("If-Unmodified-Since", condHeaders.IfUnmodifiedSince);
			}
			outputCondition.IfNotModifiedSince = new DateTime?((nullable.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable.Value.AddSeconds(1) : nullable.Value));
			outputCondition.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MayExist);
			return true;
		}
	}
}