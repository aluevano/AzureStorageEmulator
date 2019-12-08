using System;
using System.Collections.Generic;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Table.Service.DataModel
{
	public interface IUtilityTableDataContext : IDataServiceUpdateProvider, IUpdatable, IDataServiceQueryProvider, IDataServiceMetadataProvider
	{
		string ApiVersion
		{
			get;
			set;
		}

		CheckPermissionDelegate CheckPermissionCallback
		{
			get;
			set;
		}

		[SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly", Justification="XTable provider requires continuation token to be set via this dictionary.")]
		Dictionary<string, string> ContinuationToken
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Table.Service.DataModel.ContinuationTokenAvailableCallback ContinuationTokenAvailableCallback
		{
			get;
			set;
		}

		int FailedCommandIndex
		{
			get;
		}

		bool IsBatchRequest
		{
			get;
			set;
		}

		bool OldMetricsTableNamesDeprecated
		{
			get;
			set;
		}

		bool OperationIsConditional
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Table.Service.DataModel.QueryRowCommandPropertiesAvailableCallback QueryRowCommandPropertiesAvailableCallback
		{
			get;
			set;
		}

		Microsoft.Cis.Services.Nephos.Table.Service.DataModel.RequestStartedCallback RequestStartedCallback
		{
			get;
			set;
		}

		TimeSpan Timeout
		{
			get;
			set;
		}

		void OnStartProcessingRequest(ProcessRequestArgs args);
	}
}