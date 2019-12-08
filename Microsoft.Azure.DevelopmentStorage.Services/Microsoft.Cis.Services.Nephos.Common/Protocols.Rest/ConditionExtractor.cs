using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public abstract class ConditionExtractor
	{
		private static ConditionExtractor[] extractors;

		private static ConditionExtractor[] copySourceExtractors;

		static ConditionExtractor()
		{
			ConditionExtractor[] ifModifiedConditionExtractor = new ConditionExtractor[] { new IfModifiedConditionExtractor(), new IfUnmodifiedConditionExtractor(), new IfMatchConditionExtractor(), new IfNoneMatchConditionExtractor(), new IfRangeConditionExtractor() };
			ConditionExtractor.extractors = ifModifiedConditionExtractor;
			ConditionExtractor[] copySourceIfModifiedConditionExtractor = new ConditionExtractor[] { new CopySourceIfModifiedConditionExtractor(), new CopySourceIfUnmodifiedConditionExtractor(), new CopySourceIfMatchConditionExtractor(), new CopySourceIfNoneMatchConditionExtractor() };
			ConditionExtractor.copySourceExtractors = copySourceIfModifiedConditionExtractor;
		}

		protected ConditionExtractor()
		{
		}

		private static ConditionInformation ExecuteExtractors(ConditionalHeaders condHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionExtractor[] extractors)
		{
			ConditionInformation conditionInformation;
			ConditionInformation conditionInformation1 = null;
			ConditionExtractor[] conditionExtractorArray = extractors;
			int num = 0;
		Label1:
			while (num < (int)conditionExtractorArray.Length)
			{
				ConditionExtractor conditionExtractor = conditionExtractorArray[num];
				try
				{
					if (!conditionExtractor.ProcessConditionalHeader(condHeaders, requestHeaders, operationType, requestVersion, out conditionInformation1))
					{
						goto Label0;
					}
					else
					{
						conditionInformation = conditionInformation1;
					}
				}
				catch (InvalidHeaderProtocolException invalidHeaderProtocolException)
				{
					conditionInformation = null;
				}
				return conditionInformation;
			}
			return null;
		Label0:
			num++;
			goto Label1;
		}

		public static ConditionalHeaders GetConditionalHeadersFromRequest(NameValueCollection requestHeaders)
		{
			bool flag = false;
			ConditionalHeaders conditionalHeader = new ConditionalHeaders()
			{
				IfModifiedSince = requestHeaders["If-Modified-Since"]
			};
			if (!string.IsNullOrEmpty(conditionalHeader.IfModifiedSince))
			{
				flag = true;
			}
			conditionalHeader.IfUnmodifiedSince = requestHeaders["If-Unmodified-Since"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.IfUnmodifiedSince))
			{
				flag = true;
			}
			conditionalHeader.IfMatch = requestHeaders["If-Match"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.IfMatch))
			{
				flag = true;
			}
			conditionalHeader.IfNoneMatch = requestHeaders["If-None-Match"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.IfNoneMatch))
			{
				flag = true;
			}
			if (!string.IsNullOrEmpty(requestHeaders["If-Range"]) && !string.IsNullOrEmpty(requestHeaders["Range"]))
			{
				conditionalHeader.IfRange = requestHeaders["If-Range"];
				if (!flag && !string.IsNullOrEmpty(conditionalHeader.IfRange))
				{
					flag = true;
				}
			}
			conditionalHeader.CopySourceIfModifiedSince = requestHeaders["x-ms-source-if-modified-since"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.CopySourceIfModifiedSince))
			{
				flag = true;
			}
			conditionalHeader.CopySourceIfUnmodifiedSince = requestHeaders["x-ms-source-if-unmodified-since"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.CopySourceIfUnmodifiedSince))
			{
				flag = true;
			}
			conditionalHeader.CopySourceIfMatch = requestHeaders["x-ms-source-if-match"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.CopySourceIfMatch))
			{
				flag = true;
			}
			conditionalHeader.CopySourceIfNoneMatch = requestHeaders["x-ms-source-if-none-match"];
			if (!flag && !string.IsNullOrEmpty(conditionalHeader.CopySourceIfNoneMatch))
			{
				flag = true;
			}
			if (!flag)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Verbose.Log("Conditional Headers: NO conditional headers are set on the request");
			}
			else
			{
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] ifModifiedSince = new object[] { conditionalHeader.IfModifiedSince ?? "<null>", conditionalHeader.IfUnmodifiedSince ?? "<null>", conditionalHeader.IfMatch ?? "<null>", conditionalHeader.IfNoneMatch ?? "<null>", conditionalHeader.IfRange ?? "<null>", conditionalHeader.CopySourceIfModifiedSince ?? "<null>", conditionalHeader.CopySourceIfUnmodifiedSince ?? "<null>", conditionalHeader.CopySourceIfMatch ?? "<null>", conditionalHeader.CopySourceIfNoneMatch ?? "<null>" };
				verbose.Log("Conditional Headers:[IfModifiedSince = {0}, IfUnmodifiedSince = {1}, IfMatch = {2}, IfNoneMatch = {3}, IfRange = {4}, CopySourceIfModifiedSince = {5}, CopySourceIfUnmodifiedSince = {6}, CopySourceIfMatch = {7}, CopySourceIfNoneMatch = {8}]", ifModifiedSince);
			}
			if (!flag)
			{
				return null;
			}
			return conditionalHeader;
		}

		public static ConditionInformation GetConditionInfoFromIncrementalCopyRequest(NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion)
		{
			ConditionalHeaders conditionalHeadersFromRequest = ConditionExtractor.GetConditionalHeadersFromRequest(requestHeaders);
			if (conditionalHeadersFromRequest == null)
			{
				return null;
			}
			return ConditionExtractor.ExecuteExtractors(conditionalHeadersFromRequest, requestHeaders, operationType, requestVersion, ConditionExtractor.extractors);
		}

		public static ConditionInformation GetConditionInfoFromRequest(NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion)
		{
			ConditionalHeaders conditionalHeadersFromRequest = ConditionExtractor.GetConditionalHeadersFromRequest(requestHeaders);
			if (conditionalHeadersFromRequest == null)
			{
				return null;
			}
			if (operationType == OperationTypeForConditionParsing.ReadOperation || operationType == OperationTypeForConditionParsing.WriteOperation)
			{
				return ConditionExtractor.ExecuteExtractors(conditionalHeadersFromRequest, requestHeaders, operationType, requestVersion, ConditionExtractor.extractors);
			}
			NephosAssertionException.Assert(operationType == OperationTypeForConditionParsing.CopyOperation);
			ConditionInformation copySourceIfModifiedSince = ConditionExtractor.ExecuteExtractors(conditionalHeadersFromRequest, requestHeaders, operationType, requestVersion, ConditionExtractor.extractors);
			ConditionInformation conditionInformation = ConditionExtractor.ExecuteExtractors(conditionalHeadersFromRequest, requestHeaders, operationType, requestVersion, ConditionExtractor.copySourceExtractors);
			if (copySourceIfModifiedSince == null)
			{
				return conditionInformation;
			}
			if (conditionInformation != null)
			{
				copySourceIfModifiedSince.CopySourceIfModifiedSince = conditionInformation.CopySourceIfModifiedSince;
				copySourceIfModifiedSince.CopySourceIfNotModifiedSince = conditionInformation.CopySourceIfNotModifiedSince;
				copySourceIfModifiedSince.CopySourceIfMatch = conditionInformation.CopySourceIfMatch;
				copySourceIfModifiedSince.CopySourceIfNoneMatch = conditionInformation.CopySourceIfNoneMatch;
				NephosAssertionException.Assert(copySourceIfModifiedSince.ConditionFailStatusCode == conditionInformation.ConditionFailStatusCode);
			}
			return copySourceIfModifiedSince;
		}

		public static bool IsRequestConditional(NameValueCollection requestHeaders)
		{
			return ConditionExtractor.GetConditionalHeadersFromRequest(requestHeaders) != null;
		}

		protected abstract bool ProcessConditionalHeader(ConditionalHeaders conditionalHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, out ConditionInformation ci);

		public static bool ShouldThisVersionFailMultipleConditions(string requestVersion)
		{
			string[] strArrays = new string[] { "2008-10-27", "2009-04-14" };
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				if (strArrays[i].Equals(requestVersion))
				{
					return false;
				}
			}
			return true;
		}
	}
}