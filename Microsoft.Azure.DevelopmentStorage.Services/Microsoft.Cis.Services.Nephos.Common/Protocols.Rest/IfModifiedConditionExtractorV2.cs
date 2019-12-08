using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfModifiedConditionExtractorV2 : IConditionExtractor
	{
		public IfModifiedConditionExtractorV2()
		{
		}

		public bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation outputCondition)
		{
			DateTime? nullable;
			if (!string.IsNullOrEmpty(condHeaders.IfRange))
			{
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfModifiedSince))
			{
				return false;
			}
			outputCondition.ConditionFailStatusCode = HttpStatusCode.NotModified;
			if (!HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.IfModifiedSince, out nullable))
			{
				throw new InvalidHeaderProtocolException("If-Modified-Since", condHeaders.IfModifiedSince);
			}
			outputCondition.IfModifiedSince = new DateTime?((nullable.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable.Value.AddSeconds(1) : nullable.Value));
			outputCondition.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MayExist);
			return true;
		}
	}
}