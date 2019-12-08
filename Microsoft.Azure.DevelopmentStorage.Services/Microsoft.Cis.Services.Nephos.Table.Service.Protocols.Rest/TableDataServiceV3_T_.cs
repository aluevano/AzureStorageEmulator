using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Common;

namespace Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest
{
	public class TableDataServiceV3<T> : TableDataServiceBase<T>
	where T : IUtilityTableDataContext
	{
		public TableDataServiceV3(TableProtocolHead tableProtocolHead, IAccountIdentifier identifier, IUtilityTableDataContextFactory factory, bool operationIsConditional, Dictionary<string, string> continuationToken, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, ContinuationTokenAvailableCallback continuationTokenAvailableCallback) : base(tableProtocolHead, identifier, factory, operationIsConditional, continuationToken, requestStartedCallback, checkPermissionCallback, queryRowCommandPropertiesAvailableCallback, continuationTokenAvailableCallback)
		{
		}

		public static void InitializeService(DataServiceConfiguration config)
		{
			TableDataServiceBase<T>.InitializeCommonServiceConfig(config);
			config.DataServiceBehavior.AcceptProjectionRequests = true;
			config.DataServiceBehavior.MaxProtocolVersion = DataServiceProtocolVersion.V3;
			config.DataServiceBehavior.AlwaysUseDefaultXmlNamespaceForRootElement = true;
		}
	}
}