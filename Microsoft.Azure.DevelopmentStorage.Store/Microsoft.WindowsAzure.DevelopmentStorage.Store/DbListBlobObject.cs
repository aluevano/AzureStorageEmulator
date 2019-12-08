using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Transactions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbListBlobObject : DbBlobObject, IListBlobObject, IBlobObject, IDisposable
	{
		private static object SynchronizePutBlock;

		public long? BlobAppendOffset
		{
			get
			{
				return JustDecompileGenerated_get_BlobAppendOffset();
			}
			set
			{
				JustDecompileGenerated_set_BlobAppendOffset(value);
			}
		}

		private long? JustDecompileGenerated_BlobAppendOffset_k__BackingField;

		public long? JustDecompileGenerated_get_BlobAppendOffset()
		{
			return this.JustDecompileGenerated_BlobAppendOffset_k__BackingField;
		}

		public void JustDecompileGenerated_set_BlobAppendOffset(long? value)
		{
			this.JustDecompileGenerated_BlobAppendOffset_k__BackingField = value;
		}

		static DbListBlobObject()
		{
			DbListBlobObject.SynchronizePutBlock = new object();
		}

		internal DbListBlobObject(DbBlobContainer container, string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion) : base(container, blobName, snapshot, BlobType.ListBlob, blobServiceVersion)
		{
		}

		internal DbListBlobObject(DbStorageManager storageManager, Blob blob, BlobServiceVersion blobServiceVersion) : base(storageManager, blob, blobServiceVersion)
		{
			base.LeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
		}

		public IAsyncResult BeginAppendBlock(long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, long? conditionalMaxBlobSize, long? conditionalAppendBlockPosition, AsyncCallback callback, object state)
		{
			throw new NotImplementedException();
		}

		public override IAsyncResult BeginGetBlob(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CrcStream> asyncIteratorContext = new AsyncIteratorContext<CrcStream>("DbListBlobObject.GetBlob", callback, state);
			asyncIteratorContext.Begin(this.GetBlobImpl(blobRegion, propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutBlob(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbListBlobObject.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(contentType, contentLength, serviceMetadata, applicationMetadata, inputStream, contentMD5, invokeGeneratePutBlobServiceMetadata, generatePutBlobServiceMetadata, isLargeBlockBlobRequest, sequenceNumberUpdate, overwriteOption, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public IAsyncResult BeginPutBlob(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, byte[][] blockList, BlockSource[] blockSourceList, byte[] contentMD5, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbListBlobObject.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(contentType, contentLength, serviceMetadata, applicationMetadata, blockList, blockSourceList, contentMD5, overwriteOption, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("DbListBlobObject.SnapshotBlob", callback, state);
			asyncIteratorContext.Begin(this.SnapshotBlobImpl(metadata, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public void EndAppendBlock(IAsyncResult asyncResult)
		{
			throw new NotImplementedException();
		}

		public override CrcStream EndGetBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<CrcStream> asyncIteratorContext = (AsyncIteratorContext<CrcStream>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public override void EndPutBlob(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override DateTime EndSnapshotBlob(IAsyncResult ar)
		{
			Exception exception;
			AsyncIteratorContext<DateTime> asyncIteratorContext = (AsyncIteratorContext<DateTime>)ar;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		public static byte[] FromHexString(string p)
		{
			byte[] numArray = new byte[p.Length / 2];
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				string str = p.Substring(i * 2, 2);
				numArray[i] = byte.Parse(str, NumberStyles.HexNumber);
			}
			return numArray;
		}

		private IEnumerator<IAsyncResult> GetBlobImpl(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncIteratorContext<CrcStream> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute<CrcStream>((TimeSpan param0) => {
				CrcStream streamFromByteArray;
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						BlockBlob blockBlob = base.LoadBlockBlob(dbContext);
						if (blobRegion == null)
						{
							blobRegion = new BlobRegion((long)0, blockBlob.ContentLength);
						}
						else if (blobRegion.Offset > blockBlob.ContentLength || blobRegion.Offset == blockBlob.ContentLength && blockBlob.ContentLength > (long)0)
						{
							throw new InvalidBlobRegionException(new long?(blobRegion.Offset), "Offset value is greater than the contentlength");
						}
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blockBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, blobLeaseInfo, condition, null, false);
						long offset = blobRegion.Offset;
						long num = Math.Min(offset + blobRegion.Length, blockBlob.ContentLength);
						IOrderedQueryable<CommittedBlock> committedBlocks = 
							from b in dbContext.CommittedBlocks
							where (b.AccountName == this._blob.AccountName) && (b.ContainerName == this._blob.ContainerName) && (b.BlobName == this._blob.BlobName) && (b.VersionTimestamp == this._blob.VersionTimestamp) && (long?)b.Offset + b.Length >= (long?)offset && b.Offset < num
							orderby b.Offset
							select b;
						byte[] bytesFromCommittedBlocks = this.GetBytesFromCommittedBlocks(dbContext, offset, num, committedBlocks);
						transactionScope.Complete();
						this._blob = blockBlob;
						this.LeaseInfo = blobLeaseInfo;
						streamFromByteArray = DbStorageHelper.GetStreamFromByteArray(bytesFromCommittedBlocks);
					}
				}
				return streamFromByteArray;
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbListBlobObject.GetBlob"));
			yield return asyncResult;
			context.ResultData = this._storageManager.AsyncProcessor.EndExecute<CrcStream>(asyncResult);
		}

		private IEnumerator<IAsyncResult> GetBlockListImpl(IBlobObjectCondition condition, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, AsyncIteratorContext<IBlockLists> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute<BlockLists>((TimeSpan param0) => {
				BlockLists blockList;
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						BlockBlob blockBlob = base.LoadBlockBlob(dbContext, true);
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blockBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, blobLeaseInfo, condition, null, false);
						BlockLists dbBlockCollections = new BlockLists()
						{
							BlobLastModificationTime = blockBlob.LastModificationTime.Value,
							BlobSize = blockBlob.ContentLength
						};
						if ((blockListTypes & BlockListTypes.Committed) != BlockListTypes.None)
						{
							List<IBlock> blocks = new List<IBlock>();
							if (blockBlob.HasBlock.Value)
							{
								foreach (CommittedBlock committedBlock in 
									from blck in dbContext.CommittedBlocks
									where (blck.AccountName == this._blob.AccountName) && (blck.ContainerName == this._blob.ContainerName) && (blck.BlobName == this._blob.BlobName) && (blck.VersionTimestamp == this._blob.VersionTimestamp)
									select blck)
								{
									blocks.Add(new Block(DbListBlobObject.FromHexString(committedBlock.BlockId), committedBlock.Length.Value));
								}
							}
							dbBlockCollections.CommittedBlockList = new DbListBlobObject.DbBlockCollection(blocks);
						}
						if ((blockListTypes & BlockListTypes.Uncommitted) != BlockListTypes.None)
						{
							List<IBlock> blocks1 = new List<IBlock>();
							foreach (BlockData blockDatum in 
								from blck in dbContext.BlocksData
								where (blck.AccountName == this._blob.AccountName) && (blck.ContainerName == this._blob.ContainerName) && (blck.BlobName == this._blob.BlobName) && (blck.VersionTimestamp == this._blob.VersionTimestamp) && !blck.IsCommitted
								select blck)
							{
								blocks1.Add(new Block(DbListBlobObject.FromHexString(blockDatum.BlockId), blockDatum.Length.Value));
							}
							dbBlockCollections.UncommittedBlockList = new DbListBlobObject.DbBlockCollection(blocks1);
						}
						transactionScope.Complete();
						this._blob = blockBlob;
						this.LeaseInfo = blobLeaseInfo;
						blockList = dbBlockCollections;
					}
				}
				return blockList;
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbListBlobObject.GetBlockList"));
			yield return asyncResult;
			context.ResultData = this._storageManager.AsyncProcessor.EndExecute<BlockLists>(asyncResult);
		}

		private byte[] GetBytesFromCommittedBlocks(DevelopmentStorageDbDataContext dataContext, long start, long end, IOrderedQueryable<CommittedBlock> committedBlocks)
		{
			byte[] numArray = new byte[checked((IntPtr)(end - start))];
			long num = (long)0;
			long num1 = start;
			foreach (CommittedBlock committedBlock in committedBlocks)
			{
				BlockData blockDatum = (
					from b in dataContext.BlocksData
					where (b.AccountName == this._blob.AccountName) && (b.ContainerName == this._blob.ContainerName) && (b.BlobName == this._blob.BlobName) && ((DateTime?)b.VersionTimestamp == committedBlock.BlockVersion) && b.IsCommitted && (b.BlockId == committedBlock.BlockId)
					select b).First<BlockData>();
				byte[] numArray1 = null;
				using (FileStream fileStream = new FileStream(blockDatum.FilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					fileStream.Seek(blockDatum.StartOffset.Value, SeekOrigin.Begin);
					numArray1 = new byte[checked((IntPtr)blockDatum.Length.Value)];
					fileStream.Read(numArray1, 0, (int)numArray1.Length);
				}
				long num2 = (committedBlock.Offset < start ? start - committedBlock.Offset : (long)0);
				long? length = committedBlock.Length;
				long num3 = Math.Min(length.Value - num2, end - num1);
				Buffer.BlockCopy(numArray1, (int)num2, numArray, (int)num, (int)num3);
				num1 += num3;
				num += num3;
			}
			return numArray;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IListBlobObject.BeginGetBlockList(IBlobObjectCondition condition, BlockListTypes blockListTypes, BlobServiceVersion blobServiceVersion, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = new AsyncIteratorContext<IBlockLists>("DbListBlobObject.GetBlockList", callback, state);
			asyncIteratorContext.Begin(this.GetBlockListImpl(condition, blockListTypes, blobServiceVersion, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IListBlobObject.BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, byte[] contentMD5, AsyncCallback callback, object state)
		{
			return ((IListBlobObject)this).BeginPutBlock(blockIdentifier, contentLength, inputStream, null, contentMD5, false, null, callback, state);
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IListBlobObject.BeginPutBlock(byte[] blockIdentifier, long contentLength, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool isLargeBlockBlobRequest, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbListBlobObject.PutBlock", callback, state);
			asyncIteratorContext.Begin(this.PutBlockImpl(blockIdentifier, contentLength, inputStream, contentMD5, isLargeBlockBlobRequest, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IBlockLists Microsoft.Cis.Services.Nephos.Common.Storage.IListBlobObject.EndGetBlockList(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IBlockLists> asyncIteratorContext = (AsyncIteratorContext<IBlockLists>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IListBlobObject.EndPutBlock(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				long num;
				long num1;
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						string str = null;
						BlockBlob blockBlob = base.TryLoadBlockBlob(dbContext, out str);
						DateTime utcNow = DateTime.UtcNow;
						bool flag = false;
						if (blockBlob != null)
						{
							if (blockBlob.IsCommitted.Value && overwriteOption == OverwriteOption.CreateNewOnly)
							{
								throw new BlobAlreadyExistsException();
							}
							DbBlobObject.CheckCopyState(blockBlob);
							flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, new BlobLeaseInfo(blockBlob, utcNow), condition, null, true);
							dbContext.ClearUncommittedBlocks(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName);
							dbContext.Refresh(RefreshMode.KeepChanges, blockBlob);
						}
						else
						{
							if (overwriteOption == OverwriteOption.UpdateExistingOnly)
							{
								if (condition == null || !condition.LeaseId.HasValue)
								{
									throw new ConditionNotMetException();
								}
								throw new LeaseNotPresentException();
							}
							StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
							if (string.IsNullOrEmpty(str))
							{
								str = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced();
							}
							blockBlob = new BlockBlob()
							{
								AccountName = this._blob.AccountName,
								ContainerName = this._blob.ContainerName,
								BlobName = this._blob.BlobName,
								VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
								DirectoryPath = str
							};
							dbContext.Blobs.InsertOnSubmit(blockBlob);
						}
						StorageStampHelpers.ValidatePutBlobArguments(this, contentLength, null, applicationMetadata, contentMD5, sequenceNumberUpdate, overwriteOption, condition, true, false);
						byte[] byteArrayFromStream = DbStorageHelper.GetByteArrayFromStream(inputStream, out num, false, isLargeBlockBlobRequest);
						string file = BlockBlobDataManager.WriteBytesToFile(new BlockBlobMetaInfo(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, str), byteArrayFromStream, out num1);
						Guid guid = Guid.NewGuid();
						BlockData blockDatum = new BlockData()
						{
							AccountName = this._blob.AccountName,
							ContainerName = this._blob.ContainerName,
							BlobName = this._blob.BlobName,
							VersionTimestamp = this._blob.VersionTimestamp,
							IsCommitted = false,
							BlockId = DbListBlobObject.ToHexString(guid.ToByteArray()),
							FilePath = file,
							StartOffset = new long?(num1)
						};
						dbContext.BlocksData.InsertOnSubmit(blockDatum);
						if (invokeGeneratePutBlobServiceMetadata && generatePutBlobServiceMetadata != null)
						{
							serviceMetadata = generatePutBlobServiceMetadata();
						}
						blockBlob.ContentType = contentType ?? "application/octet-stream";
						blockBlob.ContentMD5 = contentMD5;
						blockBlob.ServiceMetadata = serviceMetadata;
						blockBlob.Metadata = applicationMetadata;
						blockBlob.IsCommitted = new bool?(false);
						blockBlob.HasBlock = new bool?(false);
						blockBlob.UncommittedBlockIdLength = null;
						blockBlob.GenerationId = Guid.NewGuid().ToString();
						blockBlob.SnapshotCount = 0;
						blockDatum.Length = new long?(num);
						blockBlob.ContentLength = num;
						base.ResetBlobLeaseToAvailable(blockBlob, flag);
						dbContext.SubmitChanges();
						StringBuilder stringBuilder = new StringBuilder();
						DbListBlobObject.SerializeCommitBlockListEntry(stringBuilder, guid.ToByteArray(), BlockSource.Uncommitted);
						DateTime? nullable = null;
						dbContext.CommitBlockList(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, stringBuilder.ToString(), ref nullable);
						transactionScope.Complete();
						blockBlob.LastModificationTime = nullable;
						this._blob = blockBlob;
						this.LeaseInfo = new BlobLeaseInfo(blockBlob, utcNow);
					}
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbListBlobObject.PutBlobImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(string contentType, long contentLength, byte[] serviceMetadata, byte[] applicationMetadata, byte[][] blockList, BlockSource[] blockSourceList, byte[] contentMD5, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IStringDataEventStream verboseDebug = Logger<INormalAndDebugLogger>.Instance.VerboseDebug;
			object[] objArray = new object[] { contentType, this.ContentLength, serviceMetadata, applicationMetadata, blockList, overwriteOption, condition, base.Timeout };
			verboseDebug.Log("PutBlobImpl.PutBlobImpl({0};{1};{2};{3};{4};{5};{6};{7})", objArray);
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						string str = null;
						BlockBlob blockBlob = base.TryLoadBlockBlob(dbContext, out str);
						DateTime utcNow = DateTime.UtcNow;
						bool flag = false;
						if (blockBlob != null)
						{
							DbBlobObject.CheckCopyState(blockBlob);
							flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, new BlobLeaseInfo(blockBlob, utcNow), condition, null, true);
							if (blockBlob.IsCommitted.Value)
							{
								if (overwriteOption == OverwriteOption.CreateNewOnly)
								{
									throw new BlobAlreadyExistsException();
								}
							}
							else if (overwriteOption == OverwriteOption.UpdateExistingOnly && condition != null && !condition.IsIncludingUncommittedBlobs)
							{
								throw new ConditionNotMetException();
							}
						}
						else
						{
							if (overwriteOption == OverwriteOption.UpdateExistingOnly)
							{
								if (condition == null || !condition.LeaseId.HasValue)
								{
									throw new ConditionNotMetException();
								}
								throw new LeaseNotPresentException();
							}
							StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
							if (string.IsNullOrEmpty(str))
							{
								str = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced();
							}
							blockBlob = new BlockBlob()
							{
								AccountName = this._blob.AccountName,
								ContainerName = this._blob.ContainerName,
								BlobName = this._blob.BlobName,
								VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
								IsCommitted = new bool?(false),
								DirectoryPath = str
							};
							dbContext.Blobs.InsertOnSubmit(blockBlob);
						}
						StorageStampHelpers.ValidatePutBlockListArguments(this, contentLength, applicationMetadata, blockList, blockSourceList, contentMD5, condition, this._blobServiceVersion);
						blockBlob.ContentType = contentType ?? "application/octet-stream";
						blockBlob.ContentMD5 = contentMD5;
						blockBlob.ServiceMetadata = serviceMetadata;
						blockBlob.Metadata = applicationMetadata;
						blockBlob.HasBlock = new bool?(true);
						blockBlob.GenerationId = Guid.NewGuid().ToString();
						blockBlob.SnapshotCount = 0;
						base.ResetBlobLeaseToAvailable(blockBlob, flag);
						dbContext.SubmitChanges();
						DateTime? nullable = null;
						StringBuilder stringBuilder = new StringBuilder();
						for (int i = 0; i < (int)blockList.Length; i++)
						{
							DbListBlobObject.SerializeCommitBlockListEntry(stringBuilder, blockList[i], (blockSourceList != null ? blockSourceList[i] : BlockSource.Uncommitted));
						}
						dbContext.CommitBlockList(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, stringBuilder.ToString(), ref nullable);
						transactionScope.Complete();
						blockBlob.LastModificationTime = nullable;
						this._blob = blockBlob;
						this.LeaseInfo = new BlobLeaseInfo(blockBlob, utcNow);
					}
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbListBlobObject.PutBlob"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> PutBlockImpl(byte[] blockIdentifier, long contentLength, Stream inputStream, byte[] contentMD5, bool isLargeBlockBlobRequest, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = null;
			BlockBlob blockBlob = null;
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				long num;
				long num1;
				StorageStampHelpers.ValidatePutBlockArguments(this, blockIdentifier, contentLength, contentMD5, condition, true);
				string str = null;
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					blobContainer = base.LoadContainer(dbContext);
					blockBlob = base.TryLoadBlockBlob(dbContext, out str);
					if (blockBlob == null)
					{
						lock (DbListBlobObject.SynchronizePutBlock)
						{
							blockBlob = base.TryLoadBlockBlob(dbContext, out str);
							if (blockBlob == null)
							{
								if (string.IsNullOrEmpty(str))
								{
									str = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced();
								}
								using (DevelopmentStorageDbDataContext developmentStorageDbDataContext = DevelopmentStorageDbDataContext.GetDbContext())
								{
									Logger<INormalAndDebugLogger>.Instance.VerboseDebug.Log("PutBlockImpl: Creating record for ({0};{1};{2})", new object[] { this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName });
									StorageStampHelpers.CheckBlobName(this._blob.BlobName, this._blob.ContainerName);
									blockBlob = new BlockBlob()
									{
										AccountName = this._blob.AccountName,
										ContainerName = this._blob.ContainerName,
										BlobName = this._blob.BlobName,
										VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
										ContentLength = (long)0,
										IsCommitted = new bool?(false),
										HasBlock = new bool?(true),
										DirectoryPath = str,
										GenerationId = Guid.NewGuid().ToString(),
										SnapshotCount = 0
									};
									developmentStorageDbDataContext.Blobs.InsertOnSubmit(blockBlob);
									developmentStorageDbDataContext.SubmitChanges();
								}
							}
						}
					}
					bool flag = false;
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blockBlob, DateTime.UtcNow);
					DbBlobObject.CheckCopyState(blockBlob);
					flag = DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, blobLeaseInfo, condition, null, true);
					byte[] byteArrayFromStream = DbStorageHelper.GetByteArrayFromStream(inputStream, out num, true, isLargeBlockBlobRequest);
					string file = BlockBlobDataManager.WriteBytesToFile(new BlockBlobMetaInfo(this._blob.AccountName, this._blob.ContainerName, this._blob.BlobName, str), byteArrayFromStream, out num1);
					string hexString = DbListBlobObject.ToHexString(blockIdentifier);
					BlockData blockDatum = this.TryLoadUncommittedBlock(dbContext, hexString);
					if (blockDatum == null)
					{
						blockDatum = new BlockData()
						{
							AccountName = this._blob.AccountName,
							ContainerName = this._blob.ContainerName,
							BlobName = this._blob.BlobName,
							VersionTimestamp = this._blob.VersionTimestamp,
							IsCommitted = false,
							BlockId = hexString
						};
						dbContext.BlocksData.InsertOnSubmit(blockDatum);
					}
					blockDatum.Length = new long?(num);
					blockDatum.FilePath = file;
					blockDatum.StartOffset = new long?(num1);
					base.ResetBlobLeaseToAvailable(blockBlob, flag);
					dbContext.SubmitChanges();
					blobLeaseInfo.SetBlob(blockBlob, blobLeaseInfo.LeaseInfoValidAt);
					this.LeaseInfo = blobLeaseInfo;
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbListBlobObject.PutBlock"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private static void SerializeCommitBlockListEntry(StringBuilder builder, byte[] block, BlockSource blockSource)
		{
			builder.Append((char)((int)block.Length + 32));
			builder.Append(DbListBlobObject.ToHexString(block));
			builder.Append((char)(48 + (ushort)blockSource));
		}

		internal static StringBuilder SerializeCommitBlockListEntryFromUncommitted(List<string> blocks)
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (string block in blocks)
			{
				DbListBlobObject.SerializeCommitBlockListEntry(stringBuilder, DbListBlobObject.FromHexString(block), BlockSource.Uncommitted);
			}
			return stringBuilder;
		}

		private IEnumerator<IAsyncResult> SnapshotBlobImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<DateTime> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute<DateTime>((TimeSpan param0) => {
				DateTime value;
				StorageStampHelpers.ValidateApplicationMetadata(metadata);
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						BlockBlob blockBlob = base.LoadBlockBlob(dbContext);
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(blockBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(blockBlob, blobLeaseInfo, condition, null, false);
						DateTime? nullable = null;
						DateTime? nullable1 = null;
						dbContext.SnapshotBlockBlob(blockBlob.AccountName, blockBlob.ContainerName, blockBlob.BlobName, metadata, ref nullable, ref nullable1);
						transactionScope.Complete();
						nullable = new DateTime?(DateTime.SpecifyKind(nullable.Value, DateTimeKind.Utc));
						this._blob = blockBlob;
						this.LeaseInfo = blobLeaseInfo;
						if (nullable1.HasValue)
						{
							this._blob.LastModificationTime = new DateTime?(nullable1.Value);
						}
						value = nullable.Value;
					}
				}
				return value;
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("BlobObject.SnapshotBlobImpl"));
			yield return asyncResult;
			context.ResultData = this._storageManager.AsyncProcessor.EndExecute<DateTime>(asyncResult);
		}

		public static string ToHexString(byte[] bytes)
		{
			StringBuilder stringBuilder = new StringBuilder((int)bytes.Length * 2);
			byte[] numArray = bytes;
			for (int i = 0; i < (int)numArray.Length; i++)
			{
				byte num = numArray[i];
				int num1 = num >> 4;
				stringBuilder.Append(num1.ToString("x"));
				int num2 = num & 15;
				stringBuilder.Append(num2.ToString("x"));
			}
			return stringBuilder.ToString();
		}

		private BlockData TryLoadUncommittedBlock(DevelopmentStorageDbDataContext context, string blockId)
		{
			BlockData blockDatum = (
				from b in context.BlocksData
				where (b.AccountName == this._blob.AccountName) && (b.ContainerName == this._blob.ContainerName) && (b.BlobName == this._blob.BlobName) && (b.VersionTimestamp == this._blob.VersionTimestamp) && !b.IsCommitted && (b.BlockId == blockId)
				select b).FirstOrDefault<BlockData>();
			return blockDatum;
		}

		internal class DbBlockCollection : IBlockCollection, IEnumerable<IBlock>, IEnumerable, IDisposable
		{
			private List<IBlock> m_blocks;

			int Microsoft.Cis.Services.Nephos.Common.Storage.IBlockCollection.BlockCount
			{
				get
				{
					return this.m_blocks.Count;
				}
			}

			public DbBlockCollection(List<IBlock> blocks)
			{
				this.m_blocks = new List<IBlock>();
				this.m_blocks.AddRange(blocks);
			}

			IEnumerator<IBlock> System.Collections.Generic.IEnumerable<Microsoft.Cis.Services.Nephos.Common.Storage.IBlock>.GetEnumerator()
			{
				return this.m_blocks.GetEnumerator();
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.m_blocks.GetEnumerator();
			}

			void System.IDisposable.Dispose()
			{
			}
		}
	}
}