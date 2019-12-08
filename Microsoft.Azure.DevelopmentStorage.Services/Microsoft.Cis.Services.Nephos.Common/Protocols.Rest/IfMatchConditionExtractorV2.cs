using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class IfMatchConditionExtractorV2 : IConditionExtractor
	{
		public IfMatchConditionExtractorV2()
		{
		}

		public bool ProcessConditionalHeader(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation outputCondition)
		{
			bool? nullable;
			if (!string.IsNullOrEmpty(condHeaders.IfRange))
			{
				return false;
			}
			if (string.IsNullOrEmpty(condHeaders.IfMatch))
			{
				return false;
			}
			outputCondition.ConditionFailStatusCode = HttpStatusCode.PreconditionFailed;
			outputCondition.ResourceExistsCondition = new ResourceExistenceCondition?(ResourceExistenceCondition.MustExist);
			string ifMatch = condHeaders.IfMatch;
			string[] strArrays = new string[] { "," };
			string[] strArrays1 = ifMatch.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
			List<DateTime> dateTimes = new List<DateTime>();
			string[] strArrays2 = strArrays1;
			for (int i = 0; i < (int)strArrays2.Length; i++)
			{
				string str = strArrays2[i];
				if (Comparison.StringEqualsIgnoreCase(str, "*"))
				{
					return false;
				}
				try
				{
					dateTimes.Add(BasicHttpProcessor.GetLastModifiedTimeFromETag(str));
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
			}
			if (dateTimes.Count > 0)
			{
				outputCondition.IfMatch = dateTimes.ToArray();
			}
			if (outputCondition != null && (int)outputCondition.IfMatch.Length > 0)
			{
				return true;
			}
			return false;
		}
	}
}