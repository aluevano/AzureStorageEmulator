using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Account;
using Microsoft.Cis.Services.Nephos.Common.Authentication;
using Microsoft.Cis.Services.Nephos.Common.Authorization;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.ServiceHttpConstants;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Table.Service.DataModel;
using Microsoft.Cis.Services.Nephos.Table.Service.Protocols.Rest;
using Microsoft.Cis.Services.Nephos.Table.Service.Providers.XTable;
using Microsoft.UtilityComputing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data.Linq;
using System.Data.Services;
using System.Data.Services.Providers;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class DbTableDataContext : IUtilityTableDataContext, IDataServiceUpdateProvider, IUpdatable, IDataServiceQueryProvider, IDataServiceMetadataProvider, IDisposable
	{
		public const int MaxPartitionKeyLengthDevStore = 255;

		public const int MaxRowKeyLengthDevStore = 255;

		private TableResourceContainer m_currentResourceContainer;

		private List<ChangeDescription> changeDescriptionList = new List<ChangeDescription>();

		private Dictionary<string, ChangeDescription> changeDescriptionMap = new Dictionary<string, ChangeDescription>();

		private string batchPK;

		protected internal DevelopmentStorageDbDataContext m_dbContext;

		private bool m_operationIsConditional;

		public IAccountIdentifier AccountIdentifier
		{
			get;
			private set;
		}

		protected string AccountName
		{
			get;
			set;
		}

		public string ApiVersion
		{
			get;
			set;
		}

		public CheckPermissionDelegate CheckPermissionCallback
		{
			get;
			set;
		}

		public string ContainerName
		{
			get
			{
				return this.AccountName;
			}
		}

		public string ContainerNamespace
		{
			get
			{
				return this.AccountName;
			}
		}

		public Dictionary<string, string> ContinuationToken
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Table.Service.DataModel.ContinuationTokenAvailableCallback ContinuationTokenAvailableCallback
		{
			get;
			set;
		}

		public object CurrentDataSource
		{
			get
			{
				return this;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		public int FailedCommandIndex
		{
			get
			{
				return JustDecompileGenerated_get_FailedCommandIndex();
			}
			set
			{
				JustDecompileGenerated_set_FailedCommandIndex(value);
			}
		}

		private int JustDecompileGenerated_FailedCommandIndex_k__BackingField;

		public int JustDecompileGenerated_get_FailedCommandIndex()
		{
			return this.JustDecompileGenerated_FailedCommandIndex_k__BackingField;
		}

		private void JustDecompileGenerated_set_FailedCommandIndex(int value)
		{
			this.JustDecompileGenerated_FailedCommandIndex_k__BackingField = value;
		}

		private NameValueCollection Headers
		{
			get;
			set;
		}

		public bool IsBatchRequest
		{
			get;
			set;
		}

		public bool IsNullPropagationRequired
		{
			get
			{
				return false;
			}
		}

		public bool OldMetricsTableNamesDeprecated
		{
			get
			{
				return false;
			}
			set
			{
			}
		}

		public bool OperationIsConditional
		{
			get
			{
				return this.m_operationIsConditional;
			}
			set
			{
				this.m_operationIsConditional = value;
				this.Headers = TableDataContextHelper.GetETagHeaders(value);
			}
		}

		public Microsoft.Cis.Services.Nephos.Table.Service.DataModel.QueryRowCommandPropertiesAvailableCallback QueryRowCommandPropertiesAvailableCallback
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Table.Service.DataModel.RequestStartedCallback RequestStartedCallback
		{
			get;
			set;
		}

		public IEnumerable<ResourceSet> ResourceSets
		{
			get
			{
				List<ResourceSet> resourceSets = new List<ResourceSet>()
				{
					TableResourceContainer.GetUtilityTableResourceContainer(this.AccountName, false)
				};
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					var tableContainers = 
						from t in dbContext.TableContainers
						where t.AccountName == this.AccountName
						select new { CasePreservedTableName = t.CasePreservedTableName };
					foreach (var tableContainer in tableContainers)
					{
						resourceSets.Add(TableResourceContainer.GetUtilityRowResourceContainer(this.AccountName, tableContainer.CasePreservedTableName));
					}
				}
				return resourceSets;
			}
		}

		public IEnumerable<ServiceOperation> ServiceOperations
		{
			get
			{
				yield break;
			}
		}

		public TableSignedAccessAccountIdentifier SignedAccountIdentifier
		{
			get;
			private set;
		}

		public TimeSpan Timeout
		{
			get;
			set;
		}

		public IEnumerable<ResourceType> Types
		{
			get
			{
				List<ResourceType> resourceTypes = new List<ResourceType>();
				foreach (ResourceSet resourceSet in this.ResourceSets)
				{
					resourceTypes.Add(resourceSet.ResourceType);
				}
				return resourceTypes;
			}
		}

		public DbTableDataContext()
		{
			this.m_dbContext = DevelopmentStorageDbDataContext.GetDbContext();
		}

		public DbTableDataContext(IAccountIdentifier accountIdentifier, string resourceOwner)
		{
			if (accountIdentifier == null || string.IsNullOrEmpty(accountIdentifier.AccountName))
			{
				throw new DataServiceException(404, "Resource not found.");
			}
			if (string.IsNullOrEmpty(resourceOwner))
			{
				throw new DataServiceException(404, "Resource not found.");
			}
			if (!string.Equals(resourceOwner, accountIdentifier.AccountName, StringComparison.OrdinalIgnoreCase) && !accountIdentifier.IsAdmin)
			{
				throw new UnauthorizedAccessException();
			}
			this.m_dbContext = DevelopmentStorageDbDataContext.GetDbContext();
			this.AccountName = accountIdentifier.AccountName;
			this.AccountIdentifier = accountIdentifier;
			this.SignedAccountIdentifier = accountIdentifier as TableSignedAccessAccountIdentifier;
		}

		private void AddChangeToMap(string partitionKey, string rowKey, ChangeDescription changeDescription)
		{
			if (partitionKey == null || rowKey == null)
			{
				throw new TableServiceGeneralException(TableServiceError.PropertiesNeedValue, null);
			}
			if (this.batchPK == null)
			{
				this.batchPK = partitionKey;
			}
			else if (!string.Equals(this.batchPK, partitionKey, StringComparison.OrdinalIgnoreCase))
			{
				throw new TableServiceGeneralException(TableServiceError.CommandsInBatchActOnDifferentPartitions, null);
			}
			string key = this.GetKey(partitionKey, rowKey);
			if (this.changeDescriptionMap.ContainsKey(key))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { key };
				throw new TableBatchDuplicateRowKeyException(string.Format(invariantCulture, "A command with RowKey '{0}' is already present in the batch. An entity can appear only once in a batch.", objArray));
			}
			this.changeDescriptionMap.Add(key, changeDescription);
		}

		public void AddReferenceToCollection(object targetResource, string propertyName, object resourceToBeAdded)
		{
			throw new NotSupportedException();
		}

		private void CheckAnalyticsPermissions(string userTableName, bool? isUtilityTableCommand)
		{
			bool flag = (isUtilityTableCommand.HasValue ? isUtilityTableCommand.Value : true);
			if (!this.AccountIdentifier.IsAdmin && flag && SpecialNames.IsTableContainerSpecialName(userTableName))
			{
				throw new InsufficientAccountPermissionsException();
			}
		}

		private TableRow CheckAndGetEntity(ChangeDescription changeDescription, bool shouldExist)
		{
			UtilityRow utilityRow = this.ExecuteQuery<UtilityRow>(changeDescription);
			this.CheckPermissions(utilityRow, changeDescription);
			PointQueryTracker pointQuery = ((DbTableRowQueryProvider<UtilityRow>)changeDescription.QueryableRow.Provider).PointQuery;
			if (pointQuery != null)
			{
				this.CheckPartitionAndRowKeys(pointQuery.PartitionKey, pointQuery.RowKey, changeDescription.UpdateType);
			}
			if (utilityRow == null)
			{
				if (shouldExist)
				{
					throw new TableServiceGeneralException(TableServiceError.EntityNotFound, null);
				}
				return null;
			}
			changeDescription.ExistingRow = utilityRow;
			TableRow sqlEntity = this.GetSqlEntity(utilityRow.PartitionKey, utilityRow.RowKey, changeDescription.EtagConditionUsed);
			if (sqlEntity == null)
			{
				throw new TableServiceGeneralException(TableServiceError.UpdateConditionNotSatisfied, null);
			}
			return sqlEntity;
		}

		private void CheckPartitionAndRowKeys(string partitionKey, string rowKey, UpdateKind updateType)
		{
			if (partitionKey == null || rowKey == null)
			{
				throw new TableServiceGeneralException(TableServiceError.PropertiesNeedValue, null);
			}
			bool flag = (updateType == UpdateKind.Insert || updateType == UpdateKind.InsertOrMerge ? true : updateType == UpdateKind.InsertOrReplace);
			if (flag && partitionKey.Length > 255)
			{
				throw new TableKeyTooLargeException(KeyType.PartitionKey);
			}
			if (flag && rowKey.Length > 255)
			{
				throw new TableKeyTooLargeException(KeyType.RowKey);
			}
		}

		internal void CheckPermission(string userTableName, bool isUtilityTableCommand, bool shouldCheckGet, UpdateKind commandKind)
		{
			PermissionLevel permissionLevel;
			if (this.SignedAccountIdentifier != null)
			{
				IStringDataEventStream verboseDebug = Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug;
				object[] tableName = new object[] { this.SignedAccountIdentifier.TableName, this.SignedAccountIdentifier.StartingPartitionKey, this.SignedAccountIdentifier.StartingRowKey, this.SignedAccountIdentifier.EndingPartitionKey, this.SignedAccountIdentifier.EndingRowKey };
				verboseDebug.Log("Sas tn={0} spk={1} srk={2} epk={3} erk={4}", tableName);
			}
			List<SASPermission> sASPermissions = new List<SASPermission>();
			if (!shouldCheckGet)
			{
				switch (commandKind)
				{
					case UpdateKind.Insert:
					{
						if (!isUtilityTableCommand)
						{
							sASPermissions.Add(SASPermission.Add);
						}
						else
						{
							sASPermissions.Add(SASPermission.Create);
							sASPermissions.Add(SASPermission.Write);
						}
						permissionLevel = PermissionLevel.Write;
						this.CheckAnalyticsPermissions(userTableName, null);
						goto Label0;
					}
					case UpdateKind.Delete:
					{
						sASPermissions.Add(SASPermission.Delete);
						permissionLevel = PermissionLevel.Delete;
						this.CheckAnalyticsPermissions(userTableName, new bool?(isUtilityTableCommand));
						goto Label0;
					}
					case UpdateKind.Replace:
					case UpdateKind.Merge:
					{
						sASPermissions.Add(SASPermission.Update);
						permissionLevel = PermissionLevel.Write;
						this.CheckAnalyticsPermissions(userTableName, null);
						goto Label0;
					}
					case UpdateKind.InsertOrMerge:
					case UpdateKind.InsertOrReplace:
					{
						sASPermissions.Add(SASPermission.Add | SASPermission.Update);
						permissionLevel = PermissionLevel.Write;
						this.CheckAnalyticsPermissions(userTableName, null);
						goto Label0;
					}
				}
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] accountName = new object[] { this.AccountName, commandKind, this.m_currentResourceContainer.Name };
				throw new NephosUnauthorizedAccessException(string.Format(invariantCulture, "Account {0} is not authorized to perform operation {1} on table {2}.", accountName));
			}
			else
			{
				if (!isUtilityTableCommand)
				{
					sASPermissions.Add(SASPermission.Read);
				}
				else
				{
					sASPermissions.Add(SASPermission.List);
				}
				permissionLevel = PermissionLevel.Read;
			}
		Label0:
			this.CheckPermissionCallback(permissionLevel);
			if (this.SignedAccountIdentifier != null)
			{
				SASPermission item = SASPermission.None;
				if (sASPermissions.Count > 0)
				{
					item = sASPermissions[0];
				}
				XfeTableSASAuthorizationManager.AuthorizeSASRequest(this.SignedAccountIdentifier, PermissionLevel.Write, item, userTableName, isUtilityTableCommand);
				return;
			}
			if (this.AccountIdentifier is AccountSasAccessIdentifier)
			{
				string lower = userTableName.ToLower();
				if (isUtilityTableCommand)
				{
					lower = "Tables";
				}
				if (!string.Equals(this.m_currentResourceContainer.Name, lower, StringComparison.OrdinalIgnoreCase))
				{
					throw new NephosUnauthorizedAccessException("Signed access not supported for this request as table name did not match", AuthorizationFailureReason.InvalidOperationSAS);
				}
				AccountSasAccessIdentifier accountIdentifier = this.AccountIdentifier as AccountSasAccessIdentifier;
				AuthorizationResult authorizationResult = new AuthorizationResult(false, AuthorizationFailureReason.UnauthorizedAccountSasRequest);
				SASAuthorizationParameters sASAuthorizationParameter = new SASAuthorizationParameters()
				{
					SignedResourceType = (isUtilityTableCommand ? SasResourceType.Container : SasResourceType.Object),
					SupportedSasTypes = SasType.AccountSas
				};
				SASAuthorizationParameters item1 = sASAuthorizationParameter;
				for (int i = 0; !authorizationResult.Authorized && i < sASPermissions.Count; i++)
				{
					item1.SignedPermission = sASPermissions[i];
					authorizationResult = AuthorizationManager.AuthorizeAccountSignedAccessRequest(accountIdentifier, this.AccountName, item1);
				}
				if (!authorizationResult.Authorized)
				{
					throw new NephosUnauthorizedAccessException("Signed access insufficient permission", authorizationResult.FailureReason);
				}
			}
		}

		private void CheckPermissions(UtilityRow existingRow, ChangeDescription changeDescription)
		{
			string partitionKey = null;
			string rowKey = null;
			if (existingRow != null)
			{
				partitionKey = existingRow.PartitionKey;
				rowKey = existingRow.RowKey;
			}
			else
			{
				partitionKey = ((UtilityRow)changeDescription.Row).PartitionKey;
				rowKey = ((UtilityRow)changeDescription.Row).RowKey;
			}
			this.CheckPermission(this.m_currentResourceContainer.Name, false, false, changeDescription.UpdateType);
			XfeTableSASAuthorizationManager.CheckSignedAccessKeyBoundary(this.SignedAccountIdentifier, partitionKey, rowKey);
		}

		public void ClearChanges()
		{
			this.changeDescriptionList.Clear();
		}

		private TableRow CreateAndPopulateSqlEntity(UtilityRow utilRow)
		{
			TableRow tableRow = new TableRow()
			{
				AccountName = this.AccountName,
				TableName = this.m_currentResourceContainer.Name.ToLowerInvariant(),
				PartitionKey = DevelopmentStorageDbDataContext.EncodeKeyString(utilRow.PartitionKey),
				RowKey = DevelopmentStorageDbDataContext.EncodeKeyString(utilRow.RowKey),
				Data = XmlUtility.GetXmlFromUtilityRow(utilRow)
			};
			return tableRow;
		}

		public object CreateResource(string containerName, string fullTypeName)
		{
			ChangeDescription changeDescription = new ChangeDescription(this.AccountName, containerName)
			{
				UpdateType = UpdateKind.Insert
			};
			if (!TableResourceContainer.IsUtilityTables(containerName))
			{
				changeDescription.Row = new UtilityRow();
			}
			else
			{
				changeDescription.Row = new UtilityTable();
			}
			this.changeDescriptionList.Add(changeDescription);
			return changeDescription;
		}

		public void DeleteResource(object targetResource)
		{
			if (targetResource == null)
			{
				throw new ArgumentNullException("targetResource");
			}
			((ChangeDescription)targetResource).UpdateType = UpdateKind.Delete;
		}

		public void Dispose()
		{
		}

		private T ExecuteQuery<T>(ChangeDescription changeDescription)
		{
			((DbTableRowQueryProvider<T>)changeDescription.QueryableRow.Provider).CheckForReadPermission = false;
			DbTableRowQueryProvider<T> provider = (DbTableRowQueryProvider<T>)changeDescription.QueryableRow.Provider;
			IEnumerator<T> enumerator = provider.Execute<IEnumerator<T>>(changeDescription.QueryableRow.Expression);
			if (!enumerator.MoveNext())
			{
				return default(T);
			}
			T current = enumerator.Current;
			if (enumerator.MoveNext())
			{
				throw new DataServiceException(400, "Multiple objects match the query");
			}
			return current;
		}

		public IEnumerable<ResourceType> GetDerivedTypes(ResourceType resourceType)
		{
			yield break;
		}

		private string GetKey(string pk, string rk)
		{
			return string.Format("{0}_{1}", pk, rk);
		}

		public object GetOpenPropertyValue(object target, string propertyName)
		{
			throw new NotSupportedException("single row/single property projection is not supported");
		}

		public IEnumerable<KeyValuePair<string, object>> GetOpenPropertyValues(object target)
		{
			UtilityRow utilityRow = target as UtilityRow;
			if (utilityRow == null)
			{
				return null;
			}
			return utilityRow.ColumnValues.AsEnumerable<KeyValuePair<string, object>>();
		}

		public object GetPropertyValue(object target, ResourceProperty resourceProperty)
		{
			string name = resourceProperty.Name;
			UtilityRow utilityRow = target as UtilityRow;
			if (utilityRow != null)
			{
				return utilityRow[name];
			}
			return (target as UtilityTable)[name];
		}

		public IQueryable GetQueryRootForResourceSet(ResourceSet resourceSet)
		{
			TableResourceContainer tableResourceContainer = resourceSet as TableResourceContainer;
			if (TableResourceContainer.IsUtilityTables(tableResourceContainer.Name))
			{
				return DbQueryBuilder.CreateTableQuery(this.AccountName, this);
			}
			return DbQueryBuilder.CreateRowQuery(this.AccountName, tableResourceContainer.Name.ToLowerInvariant(), this);
		}

		public object GetResource(IQueryable query, string fullTypeName)
		{
			ChangeDescription changeDescription = null;
			changeDescription = new ChangeDescription(this.AccountName, this.m_currentResourceContainer.Name)
			{
				UpdateType = UpdateKind.Merge
			};
			if (!TableResourceContainer.IsUtilityTables(this.m_currentResourceContainer.Name))
			{
				changeDescription.Row = new UtilityRow();
			}
			else
			{
				changeDescription.Row = new UtilityTable();
			}
			changeDescription.QueryableRow = query;
			this.changeDescriptionList.Add(changeDescription);
			return changeDescription;
		}

		public ResourceAssociationSet GetResourceAssociationSet(ResourceSet resourceSet, ResourceType resourceType, ResourceProperty resourceProperty)
		{
			throw new NotImplementedException();
		}

		public ResourceType GetResourceType(object target)
		{
			return this.m_currentResourceContainer.ResourceType;
		}

		private TableRow GetSqlEntity(string partitionKey, string rowKey, DateTime? timeStamp)
		{
			string str = partitionKey;
			string str1 = rowKey;
			TableRow tableRow = null;
			str = DevelopmentStorageDbDataContext.EncodeKeyString(str);
			str1 = DevelopmentStorageDbDataContext.EncodeKeyString(str1);
			if (!timeStamp.HasValue || !timeStamp.HasValue)
			{
				tableRow = (
					from r in this.m_dbContext.TableRows
					where (r.AccountName == this.AccountName) && (r.TableName == this.m_currentResourceContainer.Name.ToLowerInvariant()) && (r.PartitionKey == str) && (r.RowKey == str1)
					select r).FirstOrDefault<TableRow>();
			}
			else
			{
				DateTime sql2005DateTime = DevelopmentStorageDbDataContext.GetSql2005DateTime(timeStamp.Value);
				tableRow = (
					from r in this.m_dbContext.TableRows
					where (r.AccountName == this.AccountName) && (r.TableName == this.m_currentResourceContainer.Name.ToLowerInvariant()) && (r.PartitionKey == str) && (r.RowKey == str1) && (r.Timestamp == sql2005DateTime)
					select r).FirstOrDefault<TableRow>();
			}
			return tableRow;
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer GetSqlTableContainer(string tableName)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer = (
				from c in this.m_dbContext.TableContainers
				where (c.AccountName == this.AccountName) && (c.TableName == tableName.ToLowerInvariant())
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer>();
			return tableContainer;
		}

		public object GetValue(object targetResource, string propertyName)
		{
			ChangeDescription changeDescription = (ChangeDescription)targetResource;
			if (TableResourceContainer.IsUtilityTables(this.m_currentResourceContainer.Name))
			{
				return ((UtilityTable)changeDescription.Row)[propertyName];
			}
			return ((UtilityRow)changeDescription.Row)[propertyName];
		}

		public bool HasDerivedTypes(ResourceType resourceType)
		{
			return false;
		}

		public object InvokeServiceOperation(ServiceOperation serviceOperation, object[] parameters)
		{
			throw new NotImplementedException();
		}

		public void OnStartProcessingRequest(ProcessRequestArgs args)
		{
		}

		private void ProcessChange(ChangeDescription changeDescription)
		{
			TableRow sqlEntity = null;
			PointQueryTracker pointQuery = null;
			DbTableDataContext failedCommandIndex = this;
			failedCommandIndex.FailedCommandIndex = failedCommandIndex.FailedCommandIndex + 1;
			switch (changeDescription.UpdateType)
			{
				case UpdateKind.Insert:
				{
					this.CheckPermissions(null, changeDescription);
					this.CheckPartitionAndRowKeys(((UtilityRow)changeDescription.Row).PartitionKey, ((UtilityRow)changeDescription.Row).RowKey, changeDescription.UpdateType);
					DateTime? nullable = null;
					sqlEntity = this.GetSqlEntity(((UtilityRow)changeDescription.Row).PartitionKey, ((UtilityRow)changeDescription.Row).RowKey, nullable);
					if (sqlEntity != null)
					{
						throw new TableServiceGeneralException(TableServiceError.EntityAlreadyExists, null);
					}
					UtilityRow row = (UtilityRow)changeDescription.Row;
					this.AddChangeToMap(DevelopmentStorageDbDataContext.EncodeKeyString(row.PartitionKey), DevelopmentStorageDbDataContext.EncodeKeyString(row.RowKey), changeDescription);
					this.m_dbContext.TableRows.InsertOnSubmit(this.CreateAndPopulateSqlEntity(row));
					return;
				}
				case UpdateKind.Delete:
				{
					if (changeDescription.IfMatchHeaderMissing)
					{
						throw new XStoreArgumentException("If-Match header is mandatory when deleting an entity.");
					}
					sqlEntity = this.CheckAndGetEntity(changeDescription, true);
					this.AddChangeToMap(sqlEntity.PartitionKey, sqlEntity.RowKey, changeDescription);
					this.m_dbContext.TableRows.DeleteOnSubmit(sqlEntity);
					return;
				}
				case UpdateKind.Replace:
				{
					sqlEntity = this.CheckAndGetEntity(changeDescription, true);
					this.AddChangeToMap(sqlEntity.PartitionKey, sqlEntity.RowKey, changeDescription);
					sqlEntity.Data = XmlUtility.GetXmlFromUtilityRow(changeDescription.Row as UtilityRow);
					return;
				}
				case UpdateKind.Merge:
				{
					sqlEntity = this.CheckAndGetEntity(changeDescription, true);
					this.AddChangeToMap(sqlEntity.PartitionKey, sqlEntity.RowKey, changeDescription);
					sqlEntity.Data = XmlUtility.MergeXmlProperties(changeDescription.Row as UtilityRow, sqlEntity.Data);
					return;
				}
				case UpdateKind.InsertOrMerge:
				{
					this.ExecuteQuery<UtilityRow>(changeDescription);
					pointQuery = ((DbTableRowQueryProvider<UtilityRow>)changeDescription.QueryableRow.Provider).PointQuery;
					if (pointQuery == null)
					{
						throw new DataServiceException(400, "PK and RK not present in the required format");
					}
					(changeDescription.Row as UtilityRow).PartitionKey = pointQuery.PartitionKey;
					(changeDescription.Row as UtilityRow).RowKey = pointQuery.RowKey;
					sqlEntity = this.CheckAndGetEntity(changeDescription, false);
					this.AddChangeToMap(DevelopmentStorageDbDataContext.EncodeKeyString(pointQuery.PartitionKey), DevelopmentStorageDbDataContext.EncodeKeyString(pointQuery.RowKey), changeDescription);
					if (sqlEntity != null)
					{
						sqlEntity.Data = XmlUtility.MergeXmlProperties(changeDescription.Row as UtilityRow, sqlEntity.Data);
						return;
					}
					this.m_dbContext.TableRows.InsertOnSubmit(this.CreateAndPopulateSqlEntity(changeDescription.Row as UtilityRow));
					return;
				}
				case UpdateKind.InsertOrReplace:
				{
					this.ExecuteQuery<UtilityRow>(changeDescription);
					pointQuery = ((DbTableRowQueryProvider<UtilityRow>)changeDescription.QueryableRow.Provider).PointQuery;
					if (pointQuery == null)
					{
						throw new DataServiceException(400, "PK and RK not present in the required format");
					}
					(changeDescription.Row as UtilityRow).PartitionKey = pointQuery.PartitionKey;
					(changeDescription.Row as UtilityRow).RowKey = pointQuery.RowKey;
					sqlEntity = this.CheckAndGetEntity(changeDescription, false);
					this.AddChangeToMap(DevelopmentStorageDbDataContext.EncodeKeyString(pointQuery.PartitionKey), DevelopmentStorageDbDataContext.EncodeKeyString(pointQuery.RowKey), changeDescription);
					if (sqlEntity != null)
					{
						sqlEntity.Data = XmlUtility.GetXmlFromUtilityRow(changeDescription.Row as UtilityRow);
						return;
					}
					this.m_dbContext.TableRows.InsertOnSubmit(this.CreateAndPopulateSqlEntity(changeDescription.Row as UtilityRow));
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public void RemoveReferenceFromCollection(object targetResource, string propertyName, object resourceToBeRemoved)
		{
			throw new NotImplementedException();
		}

		public object ResetResource(object resource)
		{
			ChangeDescription changeDescription = (ChangeDescription)resource;
			if (changeDescription.UpdateType == UpdateKind.Merge)
			{
				changeDescription.UpdateType = UpdateKind.Replace;
			}
			else if (changeDescription.UpdateType == UpdateKind.InsertOrMerge)
			{
				changeDescription.UpdateType = UpdateKind.InsertOrReplace;
			}
			return resource;
		}

		public object ResolveResource(object resource)
		{
			ChangeDescription changeDescription = (ChangeDescription)resource;
			if (TableResourceContainer.IsUtilityTables(this.m_currentResourceContainer.Name))
			{
				return (UtilityTable)changeDescription.Row;
			}
			return (UtilityRow)changeDescription.Row;
		}

		public void SaveChanges()
		{
			if (TableResourceContainer.IsUtilityTables(this.m_currentResourceContainer.Name))
			{
				this.SaveTableChanges();
				return;
			}
			this.SaveEntityChanges();
		}

		private void SaveEntityChanges()
		{
			this.FailedCommandIndex = -1;
			for (int i = 0; i < 5; i++)
			{
				this.changeDescriptionMap = new Dictionary<string, ChangeDescription>();
				this.m_dbContext.Dispose();
				this.FailedCommandIndex = -1;
				try
				{
					using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
					{
						this.m_dbContext = DevelopmentStorageDbDataContext.GetDbContext();
						foreach (ChangeDescription changeDescription in this.changeDescriptionList)
						{
							this.ProcessChange(changeDescription);
						}
						this.m_dbContext.SubmitChanges();
						transactionScope.Complete();
					}
					break;
				}
				catch (SqlException sqlException)
				{
					if (sqlException.Number != 1205)
					{
						throw;
					}
					if (i + 1 == 5)
					{
						throw;
					}
					Thread.Sleep(TimeSpan.FromMilliseconds(500));
					this.FailedCommandIndex = -1;
				}
			}
			foreach (ChangeDescription changeDescription1 in this.changeDescriptionList)
			{
				this.SetValuesInResponseIfRequired(changeDescription1);
			}
		}

		private void SaveTableChanges()
		{
			this.FailedCommandIndex = 0;
			if (this.changeDescriptionList == null || this.changeDescriptionList.Count != 1)
			{
				throw new XStoreArgumentException("Invalid number of table commands. Only a single table command is allowed.");
			}
			ChangeDescription changeDescription = this.changeDescriptionList.FirstOrDefault<ChangeDescription>();
			this.CheckPermission(changeDescription.TableName, true, false, changeDescription.UpdateType);
			if (changeDescription.UpdateType != UpdateKind.Insert)
			{
				if (changeDescription.UpdateType != UpdateKind.Delete)
				{
					throw new NotSupportedException("Operation is not supported!");
				}
				UtilityTable utilityTable = this.ExecuteQuery<UtilityTable>(changeDescription);
				if (utilityTable == null)
				{
					throw new DataServiceException(404, "Resource not found.");
				}
				changeDescription.Row = utilityTable;
				Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer sqlTableContainer = this.GetSqlTableContainer(((UtilityTable)changeDescription.Row).TableName);
				this.m_dbContext.TableContainers.DeleteOnSubmit(sqlTableContainer);
				this.m_dbContext.SubmitChanges();
				return;
			}
			Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer tableContainer = new Microsoft.WindowsAzure.DevelopmentStorage.Store.TableContainer()
			{
				AccountName = changeDescription.AccountName
			};
			if (string.IsNullOrEmpty(((UtilityTable)changeDescription.Row).TableName))
			{
				throw new TableServiceGeneralException(TableServiceError.PropertiesNeedValue, null);
			}
			tableContainer.TableName = ((UtilityTable)changeDescription.Row).TableName.ToLowerInvariant();
			tableContainer.CasePreservedTableName = ((UtilityTable)changeDescription.Row).TableName;
			this.m_dbContext.TableContainers.InsertOnSubmit(tableContainer);
			this.m_dbContext.SubmitChanges();
		}

		public void SetConcurrencyValues(object resourceCookie, bool? checkForEquality, IEnumerable<KeyValuePair<string, object>> concurrencyValues)
		{
			ChangeDescription changeDescription = (ChangeDescription)resourceCookie;
			if (!checkForEquality.HasValue)
			{
				changeDescription.IfMatchHeaderMissing = true;
				changeDescription.UpdateType = UpdateKind.InsertOrMerge;
				return;
			}
			changeDescription.IfMatchHeaderMissing = false;
			int num = 0;
			DateTime? nullable = null;
			foreach (KeyValuePair<string, object> concurrencyValue in concurrencyValues)
			{
				int num1 = num + 1;
				num = num1;
				if (num1 > 1)
				{
					throw new XStoreArgumentException("Only a single concurrencyValue is supported.");
				}
				if (!string.Equals(concurrencyValue.Key, "Timestamp"))
				{
					throw new XStoreArgumentException(string.Format("Unknown ConcurrencyValue '{0}'. Only Etag checks are supported.", concurrencyValue.Key));
				}
				nullable = new DateTime?((DateTime)concurrencyValue.Value);
			}
			changeDescription.EtagConditionUsed = nullable;
		}

		public void SetReference(object targetResource, string propertyName, object propertyValue)
		{
			throw new NotImplementedException();
		}

		public void SetValue(object targetResource, string propertyName, object propertyValue)
		{
			object obj;
			try
			{
				ChangeDescription changeDescription = (ChangeDescription)targetResource;
				if (!TableResourceContainer.IsUtilityTables(this.m_currentResourceContainer.Name))
				{
					UtilityRow row = (UtilityRow)changeDescription.Row;
					if (!propertyName.Equals("Timestamp"))
					{
						if (changeDescription.UpdateType != UpdateKind.Insert && changeDescription.UpdateType != UpdateKind.InsertOrMerge && changeDescription.UpdateType != UpdateKind.InsertOrReplace && (propertyName.Equals("PartitionKey") || propertyName.Equals("RowKey")))
						{
							throw new InvalidOperationException("Cannot update key values.");
						}
						if (!TableDataContextHelper.IsValidPropertyName(propertyName, this.ApiVersion))
						{
							throw new TableServiceGeneralException(TableServiceError.PropertyNameInvalid, null);
						}
						if (row.ColumnValues.TryGetValue(propertyName, out obj) && obj != null)
						{
							throw new XStoreArgumentException("Multiple entries for same property specified in the input");
						}
						row[propertyName] = propertyValue;
					}
					else
					{
						return;
					}
				}
				else
				{
					UtilityTable utilityTable = (UtilityTable)changeDescription.Row;
					if (changeDescription.UpdateType == UpdateKind.Insert)
					{
						utilityTable[propertyName] = propertyValue;
						if (string.Equals(propertyName, "TableName"))
						{
							if (TableResourceContainer.IsUtilityTables(propertyValue as string))
							{
								throw new TableServiceArgumentException("Invalid table container name");
							}
							if (!Regex.IsMatch(propertyValue as string, "^[A-Za-z][A-Za-z0-9]{2,62}$"))
							{
								throw new InvalidResourceNameException("Invalid table name");
							}
						}
					}
				}
			}
			catch (InvalidCastException invalidCastException1)
			{
				InvalidCastException invalidCastException = invalidCastException1;
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { propertyName };
				throw new XStoreArgumentException(string.Format(invariantCulture, "Invalid data type for property {0}", objArray), invalidCastException);
			}
		}

		private void SetValuesInResponseIfRequired(ChangeDescription changeDescription)
		{
			TableRow sqlEntity = null;
			switch (changeDescription.UpdateType)
			{
				case UpdateKind.Insert:
				case UpdateKind.Replace:
				case UpdateKind.Merge:
				case UpdateKind.InsertOrMerge:
				case UpdateKind.InsertOrReplace:
				{
					string str = (changeDescription.ExistingRow != null ? changeDescription.ExistingRow.PartitionKey : ((UtilityRow)changeDescription.Row).PartitionKey);
					sqlEntity = this.GetSqlEntity(str, (changeDescription.ExistingRow != null ? changeDescription.ExistingRow.RowKey : ((UtilityRow)changeDescription.Row).RowKey), null);
					((UtilityRow)changeDescription.Row).Timestamp = sqlEntity.Timestamp;
					return;
				}
				case UpdateKind.Delete:
				{
					return;
				}
				default:
				{
					return;
				}
			}
		}

		public bool TryResolveResourceSet(string name, out ResourceSet resourceSet)
		{
			string str;
			if (this.m_currentResourceContainer == null || !name.Equals(this.m_currentResourceContainer.ResourceType.Name, StringComparison.OrdinalIgnoreCase))
			{
				if (!TableResourceContainer.IsUtilityTables(name))
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						str = (
							from t in dbContext.TableContainers
							where (t.AccountName == this.AccountName) && (t.TableName == name.ToLowerInvariant())
							select t.CasePreservedTableName).FirstOrDefault<string>();
					}
					if (str == null)
					{
						resourceSet = null;
						throw new ContainerNotFoundException();
					}
					this.m_currentResourceContainer = TableResourceContainer.GetResourceContainer(this.AccountName, str, false);
				}
				else
				{
					this.m_currentResourceContainer = TableResourceContainer.GetResourceContainer(this.AccountName, "Tables", false);
				}
			}
			resourceSet = this.m_currentResourceContainer;
			return true;
		}

		public bool TryResolveResourceType(string name, out ResourceType resourceType)
		{
			resourceType = this.m_currentResourceContainer.ResourceType;
			return true;
		}

		public bool TryResolveServiceOperation(string name, out ServiceOperation serviceOperation)
		{
			serviceOperation = null;
			return false;
		}
	}
}