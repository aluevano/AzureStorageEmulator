using Microsoft.Cis.Services.Nephos.Common.Logging;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Transactions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlobDbUploader
	{
		private string _accountName;

		private string _containerName;

		private string _blobName;

		private DateTime _versionTimeStamp;

		private string _copyId;

		private List<string> _blockIds = new List<string>();

		private long _contentLength;

		private long _currentLength;

		private DateTime? _lastModifiedTime = null;

		private int _maxNumberOfRetries = 3;

		public BlobDbUploader(string accountName, string containerName, string blobName, DateTime versionTimeStamp, long contentLen, string copyId, DateTime? lmt)
		{
			this._accountName = accountName;
			this._containerName = containerName;
			this._blobName = blobName;
			this._versionTimeStamp = versionTimeStamp;
			this._contentLength = contentLen;
			this._copyId = copyId;
			this._lastModifiedTime = lmt;
		}

		private Blob GetBlob(DevelopmentStorageDbDataContext context, bool shouldCheckLMT)
		{
			Blob blob = (
				from b in context.Blobs
				where (b.AccountName == this._accountName) && (b.ContainerName == this._containerName) && (b.BlobName == this._blobName) && (b.VersionTimestamp == this._versionTimeStamp)
				select b).FirstOrDefault<Blob>();
			if (blob == null)
			{
				throw new BlobNotFoundException();
			}
			if (shouldCheckLMT && blob.LastModificationTime.HasValue && this._lastModifiedTime.HasValue && blob.LastModificationTime.Value.ToString("u") != this._lastModifiedTime.Value.ToString("u"))
			{
				throw new ConditionNotMetException();
			}
			return blob;
		}

		private Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer GetContainer(DevelopmentStorageDbDataContext context)
		{
			Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer blobContainer = (
				from c in context.BlobContainers
				where (c.AccountName == this._accountName) && (c.ContainerName == this._containerName)
				select c).FirstOrDefault<Microsoft.WindowsAzure.DevelopmentStorage.Store.BlobContainer>();
			if (blobContainer == null)
			{
				throw new ContainerNotFoundException();
			}
			return blobContainer;
		}

		public bool PutBlock(string blockId, byte[] data)
		{
			bool flag;
			bool flag1 = false;
			DateTime? nullable = this._lastModifiedTime;
			bool flag2 = false;
			long num = (long)0;
			string file = null;
			this._currentLength += data.LongLength;
			this._blockIds.Add(blockId);
			try
			{
				for (int i = 0; i < this._maxNumberOfRetries; i++)
				{
					this._lastModifiedTime = nullable;
					try
					{
						using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
						{
							using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								this.GetContainer(dbContext);
								BlockBlob blob = this.GetBlob(dbContext, true) as BlockBlob;
								if (!flag2)
								{
									file = BlockBlobDataManager.WriteBytesToFile(new BlockBlobMetaInfo(blob.AccountName, blob.ContainerName, blob.BlobName, blob.DirectoryPath), data, out num);
									flag2 = true;
								}
								BlockData blockDatum = new BlockData()
								{
									AccountName = blob.AccountName,
									ContainerName = blob.ContainerName,
									BlobName = blob.BlobName,
									VersionTimestamp = blob.VersionTimestamp,
									IsCommitted = false,
									BlockId = blockId,
									Length = new long?(data.LongLength),
									FilePath = file,
									StartOffset = new long?(num)
								};
								dbContext.BlocksData.InsertOnSubmit(blockDatum);
								this.SetCopyStatus(blob);
								dbContext.SubmitChanges();
								dbContext.Refresh(RefreshMode.OverwriteCurrentValues, blob);
								transactionScope.Complete();
								this._lastModifiedTime = blob.LastModificationTime;
							}
						}
						Logger<IRestProtocolHeadLogger>.Instance.Info.Log("Uploaded Block: {0}", new object[] { blockId });
						break;
					}
					catch (SqlException sqlException)
					{
						if (this.ShouldRethrowException(i, sqlException, out flag1))
						{
							throw;
						}
					}
				}
				if (this._currentLength < this._contentLength)
				{
					return true;
				}
				return this.PutBlockList();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("CopyBlob: PutBlock exception {0}", new object[] { exception });
				this.SetCopyFailed(exception, flag1, null);
				flag = false;
			}
			return flag;
		}

		private bool PutBlockList()
		{
			bool flag;
			bool flag1 = false;
			try
			{
				for (int i = 0; i < this._maxNumberOfRetries; i++)
				{
					try
					{
						using (TransactionScope transactionScope = new TransactionScope(TransactionScopeOption.RequiresNew))
						{
							using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								this.GetContainer(dbContext);
								BlockBlob blob = this.GetBlob(dbContext, true) as BlockBlob;
								BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(blob.ServiceMetadata);
								instance.CopyStatus = "success";
								instance.CopyProgressOffset = this._contentLength.ToString();
								instance.CopyProgressTotal = this._contentLength.ToString();
								instance.CopyCompletionTime = new long?(DateTime.UtcNow.ToFileTimeUtc());
								blob.HasBlock = new bool?((this._blockIds.Count > 1 ? true : this._contentLength < (long)4194304));
								blob.ContentLength = this._contentLength;
								blob.ServiceMetadata = instance.GetMetadata();
								blob.IsCommitted = new bool?(false);
								blob.UncommittedBlockIdLength = null;
								dbContext.SubmitChanges();
								dbContext.CommitBlockList(this._accountName, this._containerName, this._blobName, DbListBlobObject.SerializeCommitBlockListEntryFromUncommitted(this._blockIds).ToString(), ref this._lastModifiedTime);
								transactionScope.Complete();
							}
						}
						IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
						object[] objArray = new object[] { this._accountName, this._containerName, this._blobName, this._contentLength };
						info.Log("Commited Blocks for: {0}/{1}/{2}, Length:{3}", objArray);
						break;
					}
					catch (SqlException sqlException)
					{
						if (this.ShouldRethrowException(i, sqlException, out flag1))
						{
							throw;
						}
					}
				}
				return true;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("CopyBlob: PutBlockList exception {0}", new object[] { exception });
				this.SetCopyFailed(exception, flag1, null);
				flag = false;
			}
			return flag;
		}

		public bool PutPage(long startIndex, byte[] data)
		{
			bool flag;
			bool flag1 = false;
			this._currentLength += data.LongLength;
			bool flag2 = false;
			DateTime? nullable = this._lastModifiedTime;
			try
			{
				for (int i = 0; i < this._maxNumberOfRetries; i++)
				{
					this._lastModifiedTime = nullable;
					try
					{
						using (TransactionScope transactionScope = new TransactionScope())
						{
							using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								this.GetContainer(dbContext);
								PageBlob blob = this.GetBlob(dbContext, true) as PageBlob;
								CurrentPage currentPage = new CurrentPage()
								{
									AccountName = blob.AccountName,
									ContainerName = blob.ContainerName,
									BlobName = blob.BlobName,
									VersionTimestamp = blob.VersionTimestamp,
									StartOffset = startIndex,
									EndOffset = startIndex + data.LongLength,
									SnapshotCount = blob.SnapshotCount
								};
								dbContext.CurrentPages.InsertOnSubmit(currentPage);
								lock (DbPageBlobObject.m_writeFileLock)
								{
									if (!flag2)
									{
										using (FileStream fileStream = new FileStream(DbPageBlobObject.GetFilePath(blob.FileName), FileMode.OpenOrCreate, FileAccess.Write))
										{
											fileStream.Seek(startIndex, SeekOrigin.Begin);
											fileStream.Write(data, 0, (int)data.Length);
										}
										flag2 = true;
									}
									dbContext.SubmitChanges();
									dbContext.Refresh(RefreshMode.OverwriteCurrentValues, blob);
									this.SetCopyStatus(blob);
									dbContext.SubmitChanges();
									dbContext.Refresh(RefreshMode.OverwriteCurrentValues, blob);
									transactionScope.Complete();
									this._lastModifiedTime = blob.LastModificationTime;
								}
								IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
								object[] objArray = new object[] { startIndex, startIndex + data.LongLength - (long)1 };
								info.Log("Uploaded Page: {0}-{1}", objArray);
							}
						}
						break;
					}
					catch (SqlException sqlException)
					{
						if (this.ShouldRethrowException(i, sqlException, out flag1))
						{
							throw;
						}
					}
				}
				return true;
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("CopyBlob: PutPage exception {0}", new object[] { exception });
				this.SetCopyFailed(exception, flag1, null);
				flag = false;
			}
			return flag;
		}

		public void SetCopyCompleted()
		{
			for (int i = 0; i < this._maxNumberOfRetries; i++)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
						{
							this.GetContainer(dbContext);
							Blob blob = this.GetBlob(dbContext, false);
							BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(blob.ServiceMetadata);
							if (!string.Equals(this._copyId, instance.CopyId, StringComparison.OrdinalIgnoreCase))
							{
								Logger<IRestProtocolHeadLogger>.Instance.Info.Log("CopyBlob: SetCopyCompleted not updating status as the copyId has changed.");
							}
							else
							{
								instance.CopyStatus = "success";
								instance.CopyProgressOffset = this._contentLength.ToString();
								instance.CopyProgressTotal = this._contentLength.ToString();
								instance.CopyCompletionTime = new long?(DateTime.UtcNow.ToFileTimeUtc());
								blob.ServiceMetadata = instance.GetMetadata();
								dbContext.SubmitChanges();
								transactionScope.Complete();
							}
						}
					}
					break;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					if (i != this._maxNumberOfRetries - 1)
					{
						IStringDataEventStream critical = Logger<IRestProtocolHeadLogger>.Instance.Critical;
						object[] objArray = new object[] { exception, i + 1 };
						critical.Log("Could not set copyStatus of blob. Exception {0}!Retrying attempt {1}...", objArray);
					}
				}
			}
		}

		public void SetCopyFailed(Exception exception, bool isRetryFailure, string description)
		{
			bool flag;
			bool flag1 = true;
			for (int i = 0; i < this._maxNumberOfRetries; i++)
			{
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						Blob blob = null;
						using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
						{
							this.GetContainer(dbContext);
							Blob metadata = this.GetBlob(dbContext, false);
							DateTime? lastModificationTime = metadata.LastModificationTime;
							DateTime? nullable = this._lastModifiedTime;
							if (lastModificationTime.HasValue != nullable.HasValue)
							{
								flag = true;
							}
							else
							{
								flag = (!lastModificationTime.HasValue ? false : lastModificationTime.GetValueOrDefault() != nullable.GetValueOrDefault());
							}
							if (flag)
							{
								flag1 = false;
							}
							BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(metadata.ServiceMetadata);
							if (!string.Equals(this._copyId, instance.CopyId, StringComparison.OrdinalIgnoreCase))
							{
								Logger<IRestProtocolHeadLogger>.Instance.Info.Log("CopyBlob: SetCopyFailed not updating status as the copyId is changed.");
								break;
							}
							else
							{
								instance.CopyStatus = "failed";
								instance.CopyStatusDescription = (description == null ? string.Format("Internal Error. HasRetriedOnFailure={0}", isRetryFailure) : description);
								instance.CopyCompletionTime = new long?(DateTime.UtcNow.ToFileTimeUtc());
								if (!flag1)
								{
									metadata.ServiceMetadata = instance.GetMetadata();
									metadata.ContentLength = (long)0;
								}
								else
								{
									blob = DbBlobObject.ClearBlob(metadata, instance.GetMetadata());
									dbContext.Blobs.DeleteOnSubmit(metadata);
								}
								dbContext.SubmitChanges();
							}
						}
						if (flag1)
						{
							using (DevelopmentStorageDbDataContext developmentStorageDbDataContext = DevelopmentStorageDbDataContext.GetDbContext())
							{
								developmentStorageDbDataContext.Blobs.InsertOnSubmit(blob);
								developmentStorageDbDataContext.SubmitChanges();
							}
						}
						transactionScope.Complete();
					}
					break;
				}
				catch (Exception exception2)
				{
					Exception exception1 = exception2;
					if (i != this._maxNumberOfRetries - 1)
					{
						IStringDataEventStream critical = Logger<IRestProtocolHeadLogger>.Instance.Critical;
						object[] objArray = new object[] { exception1, i + 1 };
						critical.Log("Could not set copyStatus of blob. Exception {0}!Retrying attempt {1}...", objArray);
					}
				}
			}
		}

		private void SetCopyStatus(Blob blobObj)
		{
			if (this._currentLength < this._contentLength)
			{
				BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(blobObj.ServiceMetadata);
				instance.CopyProgressOffset = this._currentLength.ToString();
				blobObj.ServiceMetadata = instance.GetMetadata();
			}
			IStringDataEventStream info = Logger<IRestProtocolHeadLogger>.Instance.Info;
			object[] objArray = new object[] { this._accountName, this._containerName, this._blobName, this._currentLength, this._contentLength };
			info.Log("CopyStatus for {0}/{1}/{2}: {3}B of {4}B", objArray);
		}

		public static void SetCopyStatusToFailedAfterRestart()
		{
			while (true)
			{
				IEnumerable<Blob> blobs = null;
				try
				{
					using (TransactionScope transactionScope = new TransactionScope())
					{
						using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
						{
							IQueryable<Blob> blobs1 = 
								from b in dbContext.Blobs
								select b;
							blobs = blobs1.Take<Blob>(100);
							if (blobs == null || blobs.Count<Blob>() == 0)
							{
								break;
							}
							else
							{
								foreach (Blob blob in blobs)
								{
									BlobServiceMetaData instance = BlobServiceMetaData.GetInstance(blob.ServiceMetadata);
									if (!"pending".Equals(instance.CopyStatus))
									{
										continue;
									}
									instance.CopyStatus = "failed";
									instance.CopyStatusDescription = "500 InternalError \"Reset to failed during restart.\"";
									Blob blob1 = DbBlobObject.ClearBlob(blob, instance.GetMetadata());
									dbContext.Blobs.DeleteOnSubmit(blob);
									dbContext.SubmitChanges();
									using (DevelopmentStorageDbDataContext developmentStorageDbDataContext = DevelopmentStorageDbDataContext.GetDbContext())
									{
										developmentStorageDbDataContext.Blobs.InsertOnSubmit(blob1);
										developmentStorageDbDataContext.SubmitChanges();
									}
								}
							}
						}
						transactionScope.Complete();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<IRestProtocolHeadLogger>.Instance.Error.Log("SetCopyStatusToFailedAfterRestart: Failed to change copy status! Exception {0}", new object[] { exception });
				}
			}
		}

		private bool ShouldRethrowException(int attempt, Exception ex, out bool isLastRetry)
		{
			isLastRetry = false;
			if (attempt == this._maxNumberOfRetries - 1)
			{
				isLastRetry = true;
				return true;
			}
			if (!(ex is SqlException))
			{
				return true;
			}
			return (ex as SqlException).Number != 1205;
		}
	}
}