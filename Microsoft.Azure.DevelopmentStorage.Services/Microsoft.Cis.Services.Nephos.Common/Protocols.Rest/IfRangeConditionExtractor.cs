using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfRangeConditionExtractor : ConditionExtractor
	{
		public IfRangeConditionExtractor()
		{
		}

		protected override bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation outputCondition)
		{
			if (ConditionExtractor.ShouldThisVersionFailMultipleConditions(requestVersion))
			{
				outputCondition = null;
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfRange))
			{
				outputCondition = null;
				return false;
			}
			HttpRequestAccessorCommon.RemoveRangeHeaderValues(requestHeaders);
			outputCondition = null;
			return true;
		}
	}
}