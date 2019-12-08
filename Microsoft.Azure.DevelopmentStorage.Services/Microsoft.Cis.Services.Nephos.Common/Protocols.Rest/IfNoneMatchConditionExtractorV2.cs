using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfNoneMatchConditionExtractorV2 : IConditionExtractor
	{
		public IfNoneMatchConditionExtractorV2()
		{
		}

		public bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation outputCondition)
		{
			if (!string.IsNullOrEmpty(condHeaders.IfRange))
			{
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfNoneMatch))
			{
				return false;
			}
			outputCondition.ConditionFailStatusCode = HttpStatusCode.NotModified;
			string ifNoneMatch = condHeaders.IfNoneMatch;
			string[] strArrays = new string[] { "," };
			string[] strArrays1 = ifNoneMatch.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
			List<DateTime> dateTimes = new List<DateTime>();
			string[] strArrays2 = strArrays1;
			for (int i = 0; i < (int)strArrays2.Length; i++)
			{
				string str = strArrays2[i];
				if (Comparison.StringEqualsIgnoreCase(str, "*"))
				{
					if (!VersioningHelper.IsPreFebruary16OrInvalidVersion(requestVersion))
					{
						throw new UnsatisfiableConditionException();
					}
					return false;
				}
				dateTimes.Add(BasicHttpProcessor.GetLastModifiedTimeFromETag(str));
			}
			if (dateTimes.Count > 0)
			{
				outputCondition.IfNoneMatch = dateTimes.ToArray();
			}
			if (outputCondition == null)
			{
				return false;
			}
			return (int)outputCondition.IfNoneMatch.Length > 0;
		}
	}
}