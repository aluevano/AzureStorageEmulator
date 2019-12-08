using System;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Data.Linq.Mapping;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	[Database(Name="DevelopmentStore")]
	public class DevelopmentStorageDbDataContext : DataContext
	{
		private const char KeySuffix = '\u0001';

		private const char DataSuffix = '\t';

		private readonly static Regex s_goRegex;

		public static string PageBlobRoot;

		public static string BlockBlobRoot;

		public static string SqlInstance;

		public static string DatabaseName;

		public static string LogDirectory;

		public static TextWriter LinqToSqlLogStream;

		public static bool CreateViews;

		private static MappingSource mappingSource;

		public Table<Account> Accounts
		{
			get
			{
				return base.GetTable<Account>();
			}
		}

		public Table<BlobContainer> BlobContainers
		{
			get
			{
				return base.GetTable<BlobContainer>();
			}
		}

		public Table<Blob> Blobs
		{
			get
			{
				return base.GetTable<Blob>();
			}
		}

		public Table<BlockData> BlocksData
		{
			get
			{
				return base.GetTable<BlockData>();
			}
		}

		public Table<CommittedBlock> CommittedBlocks
		{
			get
			{
				return base.GetTable<CommittedBlock>();
			}
		}

		public Table<CurrentPage> CurrentPages
		{
			get
			{
				return base.GetTable<CurrentPage>();
			}
		}

		public Table<DeletedAccount> DeletedAccounts
		{
			get
			{
				return base.GetTable<DeletedAccount>();
			}
		}

		public Table<Page> Pages
		{
			get
			{
				return base.GetTable<Page>();
			}
		}

		public Table<QueueContainer> QueueContainers
		{
			get
			{
				return base.GetTable<QueueContainer>();
			}
		}

		public Table<QueueMessage> QueueMessages
		{
			get
			{
				return base.GetTable<QueueMessage>();
			}
		}

		public Table<TableContainer> TableContainers
		{
			get
			{
				return base.GetTable<TableContainer>();
			}
		}

		public Table<TableRow> TableRows
		{
			get
			{
				return base.GetTable<TableRow>();
			}
		}

		static DevelopmentStorageDbDataContext()
		{
			DevelopmentStorageDbDataContext.s_goRegex = new Regex("\nGO *", RegexOptions.IgnoreCase | RegexOptions.Compiled);
			DevelopmentStorageDbDataContext.mappingSource = new AttributeMappingSource();
		}

		public DevelopmentStorageDbDataContext(string connection) : base(connection, DevelopmentStorageDbDataContext.mappingSource)
		{
		}

		public DevelopmentStorageDbDataContext(IDbConnection connection) : base(connection, DevelopmentStorageDbDataContext.mappingSource)
		{
		}

		public DevelopmentStorageDbDataContext(string connection, MappingSource mappingSource) : base(connection, mappingSource)
		{
		}

		public DevelopmentStorageDbDataContext(IDbConnection connection, MappingSource mappingSource) : base(connection, mappingSource)
		{
		}

		public static void AdjustSqlDateTime(ref DateTime? value)
		{
			if (!value.HasValue)
			{
				return;
			}
			value = new DateTime?(DevelopmentStorageDbDataContext.FromSql2005DateTime2(value.Value));
		}

		public static void AdjustSqlDateTime(ref DateTime value)
		{
			value = DevelopmentStorageDbDataContext.FromSql2005DateTime2(value);
		}

		public static DateTime CheckSqlBoundsAndReturnAptDate(DateTime value)
		{
			if (value >= SqlDateTime.MaxValue.Value)
			{
				value = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
			}
			else if (value <= SqlDateTime.MinValue.Value)
			{
				value = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			}
			return value;
		}

		[Function(Name="dbo.ClearQueue")]
		public int ClearQueue([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string queueName)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, queueName };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.ClearUncommittedBlocks")]
		public int ClearUncommittedBlocks([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.CommitBlockList")]
		public int CommitBlockList([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="VarChar(MAX)")] string blocks, [Parameter(DbType="DateTime")] ref DateTime? lastModificationTime)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, blocks, lastModificationTime };
			IExecuteResult executeResult = base.ExecuteMethodCall(this, currentMethod, objArray);
			lastModificationTime = (DateTime?)executeResult.GetParameterValue(4);
			return (int)executeResult.ReturnValue;
		}

		[Function(Name="dbo.CopyBlockBlob")]
		public int CopyBlockBlob([Parameter(DbType="VarChar(24)")] string sourceAccountName, [Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string sourceContainerName, [Parameter(DbType="NVarChar(256)")] string sourceBlobName, [Parameter(DbType="DateTime")] DateTime? sourceVersionTimestamp, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="NVarChar(260)")] string oldDataDirectory, [Parameter(DbType="NVarChar(260)")] string newDataDirectory)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { sourceAccountName, accountName, sourceContainerName, sourceBlobName, sourceVersionTimestamp, containerName, blobName, oldDataDirectory, newDataDirectory };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.CopyPageBlob")]
		public int CopyPageBlob([Parameter(DbType="VarChar(24)")] string sourceAccountName, [Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string sourceContainerName, [Parameter(DbType="NVarChar(256)")] string sourceBlobName, [Parameter(DbType="DateTime")] DateTime? sourceVersionTimestamp, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { sourceAccountName, accountName, sourceContainerName, sourceBlobName, sourceVersionTimestamp, containerName, blobName };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		public static void CreateDatabase(bool force)
		{
			using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
			{
				bool flag = dbContext.DatabaseExists();
				if (flag)
				{
					if (force)
					{
						dbContext.DeleteDatabase();
					}
					else
					{
						return;
					}
				}
				try
				{
					dbContext.CreateDatabase();
				}
				catch (SqlException sqlException)
				{
					if (flag)
					{
						throw;
					}
					dbContext.DetachDatabase();
					dbContext.CreateDatabase();
				}
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.ForeignKeys);
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.LastModificationTimeTriggers);
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.BlobSummary);
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.CommitBlockList);
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.QueueFunctions);
				DevelopmentStorageDbDataContext.ExecuteCommands(dbContext, Resources.PageBlob);
			}
		}

		internal static string DecodeDataString(string value)
		{
			Console.WriteLine("Decoding Data: {0}", value);
			if (string.IsNullOrEmpty(value))
			{
				return value;
			}
			if (value.Length < 1 || value[value.Length - 1] != '\t')
			{
				throw new ArgumentException("Attempted to decode a data string that has not been encoded.", "value");
			}
			return XmlConvert.DecodeName(value.Substring(0, value.Length - 1));
		}

		internal static string DecodeKeyString(string value)
		{
			Console.WriteLine("Decoding Key: {0}", value);
			if (value.Length < 1 || value[value.Length - 1] != '\u0001')
			{
				throw new ArgumentException("Attempted to decode a key string that has not been encoded.", "value");
			}
			return value.Substring(0, value.Length - 1);
		}

		private void DeleteCurrentPage(CurrentPage obj)
		{
			this.DeleteCurrentPage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset));
		}

		[Function(Name="dbo.DeleteCurrentPage")]
		public int DeleteCurrentPage([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="DateTime")] DateTime? versionTimestamp, [Parameter(DbType="BigInt")] long? startOffset, [Parameter(DbType="BigInt")] long? endOffset)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, versionTimestamp, startOffset, endOffset };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		private void DeletePage(Page obj)
		{
			this.DeletePage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset), obj.FileOffset, new int?(obj.SnapshotCount));
		}

		[Function(Name="dbo.DeletePage")]
		public int DeletePage([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="DateTime")] DateTime? versionTimestamp, [Parameter(DbType="BigInt")] long? startOffset, [Parameter(DbType="BigInt")] long? endOffset, [Parameter(DbType="BigInt")] long? fileOffset, [Parameter(DbType="Int")] int? snapshotCount)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, versionTimestamp, startOffset, endOffset, fileOffset, snapshotCount };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.DeleteSnapshots")]
		public int DeleteSnapshots([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="Bit")] bool? isDeletingOnlySnapshots, [Parameter(DbType="Bit")] bool? requiresNoSnapshots)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, isDeletingOnlySnapshots, requiresNoSnapshots };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.DequeueMessages")]
		public ISingleResult<QueueMessage> DequeueMessages([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string queueName, [Parameter(DbType="Int")] int? visibilityTimeout, [Parameter(DbType="Int")] int? dequeueCount)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, queueName, visibilityTimeout, dequeueCount };
			return (ISingleResult<QueueMessage>)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		private void DetachDatabase()
		{
			using (SqlConnection sqlConnection = new SqlConnection(base.Connection.ConnectionString))
			{
				sqlConnection.Open();
				sqlConnection.ChangeDatabase("MASTER");
				SqlCommand sqlCommand = new SqlCommand()
				{
					Connection = sqlConnection,
					CommandText = "sp_detach_db",
					CommandType = CommandType.StoredProcedure
				};
				SqlCommand sqlCommand1 = sqlCommand;
				sqlCommand1.Parameters.AddWithValue("dbName", DevelopmentStorageDbDataContext.DatabaseName);
				sqlCommand1.ExecuteNonQuery();
			}
		}

		internal static string EncodeDataString(string value)
		{
			Console.WriteLine("Encoding Data: {0}", value);
			return string.Concat(XmlConvert.EncodeName(value), '\t');
		}

		internal static string EncodeKeyString(string value)
		{
			Console.WriteLine("Encoding Key: {0}", value);
			return string.Concat(value, '\u0001');
		}

		private static void ExecuteCommands(DevelopmentStorageDbDataContext context, string fileText)
		{
			string[] strArrays = DevelopmentStorageDbDataContext.s_goRegex.Split(fileText);
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				context.ExecuteCommand(str, new object[0]);
			}
		}

		private static DateTime FromSql2005DateTime2(DateTime value)
		{
			value = new DateTime(value.Ticks / (long)10000 * (long)10000, DateTimeKind.Utc);
			if (value >= SqlDateTime.MaxValue.Value)
			{
				value = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc);
			}
			return value;
		}

		public static string GetConnectionString()
		{
			return string.Format("Data Source={0};Integrated security=SSPI;Initial Catalog={1}", DevelopmentStorageDbDataContext.SqlInstance, DevelopmentStorageDbDataContext.DatabaseName);
		}

		public static DevelopmentStorageDbDataContext GetDbContext()
		{
			DevelopmentStorageDbDataContext developmentStorageDbDataContext = new DevelopmentStorageDbDataContext(DevelopmentStorageDbDataContext.GetConnectionString())
			{
				Log = DevelopmentStorageDbDataContext.LinqToSqlLogStream
			};
			return developmentStorageDbDataContext;
		}

		[Function(Name="GETUTCDATE", IsComposable=true)]
		public DateTime GetDbUTCDate()
		{
			IExecuteResult executeResult = base.ExecuteMethodCall(this, (MethodInfo)MethodBase.GetCurrentMethod(), new object[0]);
			return (DateTime)executeResult.ReturnValue;
		}

		internal static DateTime GetSql2005DateTime(DateTime value)
		{
			if (value > SqlDateTime.MaxValue.Value)
			{
				value = SqlDateTime.MaxValue.Value;
			}
			else if (value >= SqlDateTime.MinValue.Value)
			{
				value = new DateTime(value.Ticks / (long)10000 * (long)10000, DateTimeKind.Utc);
			}
			else
			{
				value = SqlDateTime.MinValue.Value;
			}
			return value;
		}

		internal static DateTime GetSqlBoundedDateTime(DateTime value)
		{
			if (value > SqlDateTime.MaxValue.Value)
			{
				value = SqlDateTime.MaxValue.Value;
			}
			else if (value < SqlDateTime.MinValue.Value)
			{
				value = SqlDateTime.MinValue.Value;
			}
			return value;
		}

		[Function(Name="dbo.GetSummaryBlobContainers", IsComposable=true)]
		public IQueryable<BlobContainer> GetSummaryBlobContainers([Parameter(DbType="NVarChar(24)")] string accountName, [Parameter(DbType="NVarChar(256)")] string prefix, [Parameter(DbType="NVarChar(256)")] string delimiter)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, prefix, delimiter };
			return base.CreateMethodCallQuery<BlobContainer>(this, currentMethod, objArray);
		}

		[Function(Name="dbo.GetSummaryBlobs", IsComposable=true)]
		public IQueryable<Blob> GetSummaryBlobs([Parameter(DbType="NVarChar(24)")] string accountName, [Parameter(DbType="NVarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string prefix, [Parameter(DbType="NVarChar(256)")] string delimiter)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, prefix, delimiter };
			return base.CreateMethodCallQuery<Blob>(this, currentMethod, objArray);
		}

		[Function(Name="dbo.GetSummaryQueueContainers", IsComposable=true)]
		public IQueryable<QueueContainer> GetSummaryQueueContainers([Parameter(DbType="NVarChar(24)")] string accountName, [Parameter(DbType="NVarChar(256)")] string prefix, [Parameter(DbType="NVarChar(256)")] string delimiter)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, prefix, delimiter };
			return base.CreateMethodCallQuery<QueueContainer>(this, currentMethod, objArray);
		}

		private void InsertCurrentPage(CurrentPage obj)
		{
			this.InsertCurrentPage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset), new int?(obj.SnapshotCount));
		}

		[Function(Name="dbo.InsertCurrentPage")]
		public int InsertCurrentPage([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="DateTime")] DateTime? versionTimestamp, [Parameter(DbType="BigInt")] long? startOffset, [Parameter(DbType="BigInt")] long? endOffset, [Parameter(DbType="Int")] int? snapshotCount)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, versionTimestamp, startOffset, endOffset, snapshotCount };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		private void InsertPage(Page obj)
		{
			this.InsertPage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset), obj.FileOffset, new int?(obj.SnapshotCount));
		}

		[Function(Name="dbo.InsertPage")]
		public int InsertPage([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="DateTime")] DateTime? versionTimestamp, [Parameter(DbType="BigInt")] long? startOffset, [Parameter(DbType="BigInt")] long? endOffset, [Parameter(DbType="BigInt")] long? fileOffset, [Parameter(DbType="Int")] int? snapshotCount)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, versionTimestamp, startOffset, endOffset, fileOffset, snapshotCount };
			return (int)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.MatchingDelimiter", IsComposable=true)]
		public string MatchingDelimiter([Parameter(DbType="NVarChar(256)")] string value, [Parameter(DbType="NVarChar(256)")] string prefix, [Parameter(DbType="NVarChar(256)")] string delimiter)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { value, prefix, delimiter };
			return (string)base.ExecuteMethodCall(this, currentMethod, objArray).ReturnValue;
		}

		[Function(Name="dbo.SnapshotBlockBlob")]
		public int SnapshotBlockBlob([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="VarBinary(MAX)")] byte[] metadata, [Parameter(DbType="DateTime")] ref DateTime? snapshotTimestamp, [Parameter(DbType="DateTime")] ref DateTime? snapshotLastModificationTime)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, metadata, snapshotTimestamp, snapshotLastModificationTime };
			IExecuteResult executeResult = base.ExecuteMethodCall(this, currentMethod, objArray);
			snapshotTimestamp = (DateTime?)executeResult.GetParameterValue(4);
			snapshotLastModificationTime = (DateTime?)executeResult.GetParameterValue(5);
			return (int)executeResult.ReturnValue;
		}

		[Function(Name="dbo.SnapshotPageBlob")]
		public int SnapshotPageBlob([Parameter(DbType="VarChar(24)")] string accountName, [Parameter(DbType="VarChar(63)")] string containerName, [Parameter(DbType="NVarChar(256)")] string blobName, [Parameter(DbType="VarBinary(MAX)")] byte[] metadata, [Parameter(DbType="NVarChar(260)")] string pageFileName, [Parameter(DbType="Int")] int? snapshotCount, [Parameter(DbType="DateTime")] ref DateTime? snapshotTimestamp, [Parameter(DbType="DateTime")] ref DateTime? snapshotLastModificationTime)
		{
			MethodInfo currentMethod = (MethodInfo)MethodBase.GetCurrentMethod();
			object[] objArray = new object[] { accountName, containerName, blobName, metadata, pageFileName, snapshotCount, snapshotTimestamp, snapshotLastModificationTime };
			IExecuteResult executeResult = base.ExecuteMethodCall(this, currentMethod, objArray);
			snapshotTimestamp = (DateTime?)executeResult.GetParameterValue(6);
			snapshotLastModificationTime = (DateTime?)executeResult.GetParameterValue(7);
			return (int)executeResult.ReturnValue;
		}

		internal static string[] SplitCommands(string fileText)
		{
			return DevelopmentStorageDbDataContext.s_goRegex.Split(fileText);
		}

		private void UpdateCurrentPage(CurrentPage obj)
		{
			this.InsertCurrentPage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset), new int?(obj.SnapshotCount));
		}

		private void UpdatePage(Page obj)
		{
			int? nullable = null;
			this.InsertPage(obj.AccountName, obj.ContainerName, obj.BlobName, new DateTime?(obj.VersionTimestamp), new long?(obj.StartOffset), new long?(obj.EndOffset), obj.FileOffset, nullable);
		}
	}
}