using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Services;

namespace Microsoft.Cis.Services.Nephos.Table.Service.TableManager
{
	public interface ITableManager : IDisposable
	{
		Microsoft.Cis.Services.Nephos.Common.OperationStatus OperationStatus
		{
			get;
			set;
		}

		IAsyncResult BeginGetTableAcl(IAccountIdentifier identifier, string account, string tableName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		IAsyncResult BeginGetTableServiceProperties(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		IAsyncResult BeginGetTableServiceStats(IAccountIdentifier identifier, string ownerAccountName, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		IAsyncResult BeginPerformOperation(IAccountIdentifier accountId, string accountName, IDataServiceHost host, RequestStartedCallback requestStartedCallback, CheckPermissionDelegate checkPermissionCallback, QueryRowCommandPropertiesAvailableCallback queryRowCommandPropertiesAvailableCallback, Dictionary<string, string> continuationToken, ContinuationTokenAvailableCallback continuationTokenCallback, bool operationIsConditional, RequestContext requestContext, AsyncCallback callback, object state);

		IAsyncResult BeginSetTableAcl(IAccountIdentifier identifier, string account, string tableName, ContainerAclSettings acl, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		IAsyncResult BeginSetTableServiceProperties(IAccountIdentifier identifier, string ownerAccountName, AnalyticsSettings settings, TimeSpan timeout, RequestContext requestContext, AsyncCallback callback, object state);

		ContainerAclSettings EndGetTableAcl(IAsyncResult ar);

		AnalyticsSettings EndGetTableServiceProperties(IAsyncResult ar);

		GeoReplicationStats EndGetTableServiceStats(IAsyncResult ar);

		void EndPerformOperation(IAsyncResult ar);

		void EndSetTableAcl(IAsyncResult ar);

		void EndSetTableServiceProperties(IAsyncResult ar);

		void Initialize();

		void Shutdown();
	}
}