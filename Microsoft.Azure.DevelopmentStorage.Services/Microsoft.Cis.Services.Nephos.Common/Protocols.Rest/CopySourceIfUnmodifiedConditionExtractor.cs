using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections.Specialized;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public class CopySourceIfUnmodifiedConditionExtractor : ConditionExtractor
	{
		public CopySourceIfUnmodifiedConditionExtractor()
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
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfUnmodifiedSince) || !string.IsNullOrEmpty(condHeaders.CopySourceIfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfModifiedSince) || !string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch))
				{
					outputCondition = null;
					return false;
				}
			}
			else
			{
				if (string.IsNullOrEmpty(condHeaders.CopySourceIfUnmodifiedSince))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfMatch))
				{
					outputCondition = null;
					return false;
				}
				if (!string.IsNullOrEmpty(condHeaders.CopySourceIfModifiedSince) || !string.IsNullOrEmpty(condHeaders.CopySourceIfNoneMatch))
				{
					throw new MultipleConditionHeaderProtocolException();
				}
			}
			ConditionInformation conditionInformation = new ConditionInformation()
			{
				ConditionFailStatusCode = HttpStatusCode.PreconditionFailed
			};
			if (!HttpUtilities.TryGetDateTimeFromHttpString(condHeaders.CopySourceIfUnmodifiedSince, out nullable))
			{
				throw new InvalidHeaderProtocolException("x-ms-source-if-unmodified-since", condHeaders.CopySourceIfUnmodifiedSince);
			}
			conditionInformation.CopySourceIfNotModifiedSince = new DateTime?((nullable.Value < DateTime.MaxValue.AddSeconds(-1) ? nullable.Value.AddSeconds(1) : nullable.Value));
			outputCondition = conditionInformation;
			return true;
		}
	}
}