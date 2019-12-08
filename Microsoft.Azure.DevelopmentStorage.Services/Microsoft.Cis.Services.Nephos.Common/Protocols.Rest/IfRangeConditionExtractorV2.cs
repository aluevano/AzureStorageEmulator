using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfRangeConditionExtractorV2 : IConditionExtractor
	{
		public IfRangeConditionExtractorV2()
		{
		}

		public bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation outputCondition)
		{
			if (ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfRange))
			{
				return false;
			}
			HttpRequestAccessorCommon.RemoveRangeHeaderValues(requestHeaders);
			return true;
		}
	}
}