using System;
using System.Collections.Specialized;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public interface IConditionExtractor
	{
		bool ProcessConditionalHeader(ConditionalHeaders conditionalHeaders, NameValueCollection requestHeaders, OperationTypeForConditionParsing operationType, string requestVersion, ConditionInformation ci);
	}
}