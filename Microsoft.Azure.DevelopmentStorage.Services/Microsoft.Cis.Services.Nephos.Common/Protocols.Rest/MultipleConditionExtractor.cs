using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class MultipleConditionExtractor
	{
		private static IConditionExtractor[] Extractors;

		static MultipleConditionExtractor()
		{
			IConditionExtractor[] ifModifiedConditionExtractorV2 = new IConditionExtractor[] { new IfModifiedConditionExtractorV2(), new IfNoneMatchConditionExtractorV2(), new IfUnmodifiedConditionExtractorV2(), new IfMatchConditionExtractorV2(), new IfRangeConditionExtractorV2() };
			MultipleConditionExtractor.Extractors = ifModifiedConditionExtractorV2;
		}

		public MultipleConditionExtractor()
		{
		}

		private static ConditionInformation ExecuteExtractors(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, IConditionExtractor[] extractors)
		{
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				IsMultipleConditionalHeaderEnabled = true
			};
			IConditionExtractor[] conditionExtractorArray = extractors;
			for (int i = 0; i < (int)conditionExtractorArray.Length; i++)
			{
				IConditionExtractor conditionExtractor = conditionExtractorArray[i];
				try
				{
					conditionExtractor.ProcessConditionalHeader(condHeaders, requestHeaders, operationType, requestVersion, conditionInformation);
				}
				catch (InvalidHeaderProtocolException invalidHeaderProtocolException)
				{
				}
			}
			return conditionInformation;
		}

		public static ConditionInformation GetConditionInfoFromRequest(NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion)
		{
			ConditionalHeaders conditionalHeadersFromRequest = ConditionExtractor.GetConditionalHeadersFromRequest(requestHeaders);
			if (conditionalHeadersFromRequest == null)
			{
				return null;
			}
			IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] ifModifiedSince = new object[] { conditionalHeadersFromRequest.IfModifiedSince ?? "<null>", conditionalHeadersFromRequest.IfUnmodifiedSince ?? "<null>", conditionalHeadersFromRequest.IfMatch ?? "<null>", conditionalHeadersFromRequest.IfNoneMatch ?? "<null>", conditionalHeadersFromRequest.IfRange ?? "<null>", conditionalHeadersFromRequest.CopySourceIfModifiedSince ?? "<null>", conditionalHeadersFromRequest.CopySourceIfUnmodifiedSince ?? "<null>", conditionalHeadersFromRequest.CopySourceIfMatch ?? "<null>", conditionalHeadersFromRequest.CopySourceIfNoneMatch ?? "<null>" };
			info.Log("Conditional Headers:[IfModifiedSince = {0}, IfUnmodifiedSince = {1}, IfMatch = {2}, IfNoneMatch = {3}, IfRange = {4}, CopySourceIfModifiedSince = {5}, CopySourceIfUnmodifiedSince = {6}, CopySourceIfMatch = {7}, CopySourceIfNoneMatch = {8}]", ifModifiedSince);
			return MultipleConditionExtractor.ExecuteExtractors(conditionalHeadersFromRequest, requestHeaders, operationType, requestVersion, MultipleConditionExtractor.Extractors);
		}
	}
}