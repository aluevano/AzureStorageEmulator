using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Storage;
using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Linq;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Transactions;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DbPageBlobObject : DbBlobObject, IIndexBlobObject, IBlobObject, IDisposable
	{
		internal static object m_writeFileLock;

		public override long? ContentLength
		{
			get
			{
				return ((PageBlob)this._blob).MaxSize;
			}
		}

		bool Microsoft.Cis.Services.Nephos.Common.Storage.IBlobObject.IsIncrementalCopy
		{
			get
			{
				bool? isIncrementalCopy = ((PageBlob)this._blob).IsIncrementalCopy;
				if (!isIncrementalCopy.HasValue)
				{
					return false;
				}
				return isIncrementalCopy.GetValueOrDefault();
			}
		}

		static DbPageBlobObject()
		{
			DbPageBlobObject.m_writeFileLock = new object();
		}

		internal DbPageBlobObject(DbBlobContainer container, string blobName, DateTime snapshot, BlobServiceVersion blobServiceVersion) : base(container, blobName, snapshot, BlobType.IndexBlob, blobServiceVersion)
		{
		}

		internal DbPageBlobObject(DbStorageManager storageManager, Blob blob, BlobServiceVersion blobServiceVersion) : base(storageManager, blob, blobServiceVersion)
		{
			base.LeaseInfo = new BlobLeaseInfo(blob, DateTime.UtcNow);
		}

		public override IAsyncResult BeginGetBlob(IBlobRegion blobRegion, BlobPropertyNames propertyNames, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<CrcStream> asyncIteratorContext = new AsyncIteratorContext<CrcStream>("DbPageBlobObject.GetBlob", callback, state);
			asyncIteratorContext.Begin(this.GetBlobImpl(blobRegion, propertyNames, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginPutBlob(string contentType, long contentLength, long? maxBlobSize, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, CrcReaderStream crcReaderStream, byte[] contentMD5, bool invokeGeneratePutBlobServiceMetadata, GeneratePutBlobServiceMetadata generatePutBlobServiceMetadata, bool isLargeBlockBlobRequest, bool is8TBPageBlobAllowed, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbPageBlobObject.PutBlob", callback, state);
			asyncIteratorContext.Begin(this.PutBlobImpl(contentType, contentLength, maxBlobSize, is8TBPageBlobAllowed, serviceMetadata, applicationMetadata, inputStream, contentMD5, sequenceNumberUpdate, overwriteOption, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			return this.BeginSnapshotBlob(metadata, condition, callback, state, true);
		}

		public IAsyncResult BeginSnapshotBlob(byte[] metadata, IBlobObjectCondition condition, AsyncCallback callback, object state, bool throwIfRootBlob)
		{
			AsyncIteratorContext<DateTime> asyncIteratorContext = new AsyncIteratorContext<DateTime>("DbPageBlobObject.SnapshotBlob", callback, state);
			asyncIteratorContext.Begin(this.SnapshotBlobImpl(metadata, condition, asyncIteratorContext, throwIfRootBlob));
			return asyncIteratorContext;
		}

		internal static void ClearPageHelper(DevelopmentStorageDbDataContext dataContext, PageBlob blob, long start, long end)
		{
			CurrentPage currentPage = new CurrentPage()
			{
				AccountName = blob.AccountName,
				ContainerName = blob.ContainerName,
				BlobName = blob.BlobName,
				VersionTimestamp = blob.VersionTimestamp,
				StartOffset = start,
				EndOffset = end
			};
			CurrentPage currentPage1 = currentPage;
			dataContext.CurrentPages.Attach(currentPage1);
			dataContext.CurrentPages.DeleteOnSubmit(currentPage1);
			byte[] numArray = new byte[checked((IntPtr)(end - start))];
			lock (DbPageBlobObject.m_writeFileLock)
			{
				using (FileStream fileStream = new FileStream(DbPageBlobObject.GetFilePath(blob.FileName), FileMode.Open, FileAccess.Write))
				{
					fileStream.Seek(start, SeekOrigin.Begin);
					fileStream.Write(numArray, 0, (int)numArray.Length);
				}
			}
		}

		private IEnumerator<IAsyncResult> ClearPageImpl(IBlobRegion blobRegion, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					base.LoadContainer(dbContext);
					PageBlob pageBlob = base.LoadPageBlob(dbContext);
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
					DbBlobObject.CheckCopyState(pageBlob);
					bool flag = DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, true);
					long offset = blobRegion.Offset;
					long? maxSize = pageBlob.MaxSize;
					if ((offset < maxSize.GetValueOrDefault() ? true : !maxSize.HasValue))
					{
						long num = blobRegion.Offset + blobRegion.Length;
						long? nullable = pageBlob.MaxSize;
						if ((num <= nullable.GetValueOrDefault() ? true : !nullable.HasValue))
						{
							DbPageBlobObject.ClearPageHelper(dbContext, pageBlob, blobRegion.Offset, blobRegion.Offset + blobRegion.Length);
							base.ResetBlobLeaseToAvailable(pageBlob, flag);
							dbContext.SubmitChanges();
							dbContext.Refresh(RefreshMode.OverwriteCurrentValues, pageBlob);
							this._blob = pageBlob;
							this.LeaseInfo = blobLeaseInfo;
							return;
						}
					}
					throw new PageRangeInvalidException();
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.ClearPageImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private static IEnumerable<PageRange> CoalescePageRanges(IEnumerable<PageRange> pageRanges, bool enforceSameClearness)
		{
			PageRange pageRange1 = null;
			foreach (PageRange pageRange in pageRanges)
			{
				if (pageRange1 == null)
				{
					pageRange1 = pageRange;
				}
				else if (pageRange.PageStart != pageRange1.PageEnd + (long)1)
				{
					yield return pageRange1;
					pageRange1 = pageRange;
				}
				else if (enforceSameClearness && pageRange.IsClear == pageRange1.IsClear)
				{
					pageRange1 = new PageRange(pageRange1.PageStart, pageRange.PageEnd, pageRange1.IsClear);
				}
				else if (!enforceSameClearness)
				{
					pageRange1 = new PageRange(pageRange1.PageStart, pageRange.PageEnd);
				}
				else
				{
					yield return pageRange1;
					pageRange1 = pageRange;
				}
			}
			if (pageRange1 != null)
			{
				yield return pageRange1;
			}
		}

		private void CopyStream(Stream inputStream, string fileName)
		{
			Stream fileStream = new FileStream(DbPageBlobObject.GetFilePath(fileName), FileMode.Create);
			byte[] numArray = new byte[4096];
			int num = 0;
			int num1 = 0;
			do
			{
				num = inputStream.Read(numArray, 0, (int)numArray.Length);
				fileStream.Write(numArray, 0, num);
				num1 += num;
			}
			while (num1 > 0);
			fileStream.Close();
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

		private byte[] FromHexString(string p)
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
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				int num = 0;
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						PageBlob pageBlob = base.LoadPageBlob(dbContext);
						this.ThrowIfIncrementalRoot(pageBlob);
						if (blobRegion != null)
						{
							long offset = blobRegion.Offset;
							long? maxSize = pageBlob.MaxSize;
							if ((offset <= maxSize.GetValueOrDefault() ? true : !maxSize.HasValue))
							{
								long offset1 = blobRegion.Offset;
								long? nullable = pageBlob.MaxSize;
								if ((offset1 != nullable.GetValueOrDefault() ? false : nullable.HasValue))
								{
									long? maxSize1 = pageBlob.MaxSize;
									if ((maxSize1.GetValueOrDefault() <= (long)0 ? true : !maxSize1.HasValue))
									{
										goto Label0;
									}
								}
								else
								{
									goto Label0;
								}
							}
							throw new InvalidBlobRegionException(pageBlob.MaxSize, "Offset value is greater than the contentlength");
						}
						else
						{
							blobRegion = new BlobRegion((long)0, pageBlob.MaxSize.Value);
						}
					Label0:
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, false);
						long num1 = blobRegion.Offset;
						byte[] numArray = new byte[checked((IntPtr)(Math.Min(num1 + blobRegion.Length, pageBlob.MaxSize.Value) - num1))];
						using (FileStream fileStream = new FileStream(DbPageBlobObject.GetFilePath(pageBlob.FileName), FileMode.Open, FileAccess.Read))
						{
							fileStream.Seek(num1, SeekOrigin.Begin);
							for (int i = 0; i < (int)numArray.Length; i += num)
							{
								num = fileStream.Read(numArray, i, (int)numArray.Length - i);
								if (num == 0)
								{
									break;
								}
							}
						}
						this._blob = pageBlob;
						this.LeaseInfo = blobLeaseInfo;
						context.ResultData = DbStorageHelper.GetStreamFromByteArray(numArray);
						transactionScope.Complete();
					}
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.GetBlobImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		internal static string GetFilePath(string fileName)
		{
			return Path.Combine(DevelopmentStorageDbDataContext.PageBlobRoot, fileName);
		}

		private IEnumerator<IAsyncResult> GetPageRangeListImpl(IBlobRegion blobRegion, IBlobObjectCondition condition, int maxPageRanges, AsyncIteratorContext<IPageRangeCollection> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						PageBlob pageBlob = base.LoadPageBlob(dbContext);
						this.ThrowIfIncrementalRoot(pageBlob);
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, false);
						long offset = blobRegion.Offset;
						long? maxSize = pageBlob.MaxSize;
						if ((offset <= maxSize.GetValueOrDefault() ? true : !maxSize.HasValue))
						{
							long num = blobRegion.Offset;
							long? nullable = pageBlob.MaxSize;
							if ((num != nullable.GetValueOrDefault() ? false : nullable.HasValue))
							{
								long? maxSize1 = pageBlob.MaxSize;
								if ((maxSize1.GetValueOrDefault() <= (long)0 ? false : maxSize1.HasValue))
								{
									throw new InvalidBlobRegionException(pageBlob.MaxSize);
								}
							}
							IQueryable<PageRange> pageRange = null;
							if (pageBlob.VersionTimestamp < SqlDateTime.MaxValue.Value)
							{
								IQueryable<Page> pages = 
									from page in dbContext.Pages
									where (page.AccountName == this._blob.AccountName) && (page.ContainerName == this._blob.ContainerName) && (page.BlobName == this._blob.BlobName) && (page.VersionTimestamp == this._blob.VersionTimestamp)
									select page;
								long offset1 = blobRegion.Offset + blobRegion.Length - (long)1;
								pages = 
									from page in pages
									where page.StartOffset >= blobRegion.Offset && page.StartOffset < offset1 || page.EndOffset > blobRegion.Offset && page.EndOffset <= offset1
									select page;
								pageRange = 
									from page in pages
									select new PageRange(Math.Max(page.StartOffset, blobRegion.Offset), Math.Min(page.EndOffset - (long)1, offset1));
							}
							else
							{
								IQueryable<CurrentPage> currentPages = 
									from page in dbContext.CurrentPages
									where (page.AccountName == this._blob.AccountName) && (page.ContainerName == this._blob.ContainerName) && (page.BlobName == this._blob.BlobName) && (page.VersionTimestamp == this._blob.VersionTimestamp)
									select page;
								long num1 = blobRegion.Offset + blobRegion.Length - (long)1;
								currentPages = 
									from page in currentPages
									where page.StartOffset >= blobRegion.Offset && page.StartOffset < num1 || page.EndOffset > blobRegion.Offset && page.EndOffset <= num1
									select page;
								pageRange = 
									from page in currentPages
									select new PageRange(Math.Max(page.StartOffset, blobRegion.Offset), Math.Min(page.EndOffset - (long)1, num1));
							}
							IEnumerable<PageRange> pageRanges = DbPageBlobObject.CoalescePageRanges(pageRange, false);
							if (maxPageRanges != 0)
							{
								pageRanges = pageRanges.Take<PageRange>(maxPageRanges + 1);
							}
							context.ResultData = new DbPageBlobObject.DbPageRangeCollection(pageRanges, maxPageRanges);
							this._blob = pageBlob;
							this.LeaseInfo = blobLeaseInfo;
							transactionScope.Complete();
							goto Label0;
						}
						throw new InvalidBlobRegionException(pageBlob.MaxSize);
					}
				Label0:
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.GetBlockListImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute<NoResults>(asyncResult);
		}

		private IEnumerator<IAsyncResult> GetPageRangeListImpl(IBlobRegion blobRegion, IBlobObjectCondition condition, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, AsyncIteratorContext<IPageRangeCollection> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						PageBlob pageBlob = base.LoadPageBlob(dbContext);
						this.ThrowIfIncrementalRoot(pageBlob);
						Blob blob = (
							from b in dbContext.Blobs
							where (b.AccountName == pageBlob.AccountName) && (b.ContainerName == pageBlob.ContainerName) && (b.BlobName == pageBlob.BlobName) && (b.VersionTimestamp == prevSnapshotTimestamp.Value)
							select b).FirstOrDefault<Blob>();
						if (blob == null)
						{
							throw new BlobPreviousSnapshotNotFoundException();
						}
						if (pageBlob.VersionTimestamp.CompareTo(prevSnapshotTimestamp) < 0)
						{
							throw new BlobPreviousSnapshotTooNewException();
						}
						if (!pageBlob.GenerationId.Equals(blob.GenerationId))
						{
							throw new BlobGenerationIdMismatchException();
						}
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, false);
						long offset = blobRegion.Offset;
						long? maxSize = pageBlob.MaxSize;
						if ((offset <= maxSize.GetValueOrDefault() ? true : !maxSize.HasValue))
						{
							long num = blobRegion.Offset;
							long? nullable = pageBlob.MaxSize;
							if ((num != nullable.GetValueOrDefault() ? false : nullable.HasValue))
							{
								long? maxSize1 = pageBlob.MaxSize;
								if ((maxSize1.GetValueOrDefault() <= (long)0 ? false : maxSize1.HasValue))
								{
									throw new InvalidBlobRegionException(pageBlob.MaxSize);
								}
							}
							IQueryable<DbPageBlobObject.PageRangeWithSnapshot> pageRangeWithSnapshot = null;
							IQueryable<DbPageBlobObject.PageRangeWithSnapshot> pageRangeWithSnapshots = null;
							IQueryable<Page> pages = 
								from page in dbContext.Pages
								where (page.AccountName == this._blob.AccountName) && (page.ContainerName == this._blob.ContainerName) && (page.BlobName == this._blob.BlobName) && (page.VersionTimestamp == prevSnapshotTimestamp.Value)
								select page;
							long offset1 = blobRegion.Offset + blobRegion.Length - (long)1;
							pages = 
								from page in pages
								where page.StartOffset >= blobRegion.Offset && page.StartOffset < offset1 || page.EndOffset > blobRegion.Offset && page.EndOffset <= offset1
								select page;
							pageRangeWithSnapshot = 
								from page in pages
								select new DbPageBlobObject.PageRangeWithSnapshot(Math.Max(page.StartOffset, blobRegion.Offset), Math.Min(page.EndOffset - (long)1, offset1), page.SnapshotCount);
							if (pageBlob.VersionTimestamp < SqlDateTime.MaxValue.Value)
							{
								IQueryable<Page> startOffset = 
									from page in dbContext.Pages
									where (page.AccountName == this._blob.AccountName) && (page.ContainerName == this._blob.ContainerName) && (page.BlobName == this._blob.BlobName) && (page.VersionTimestamp == this._blob.VersionTimestamp)
									select page;
								startOffset = 
									from page in startOffset
									where page.StartOffset >= blobRegion.Offset && page.StartOffset < offset1 || page.EndOffset > blobRegion.Offset && page.EndOffset <= offset1
									select page;
								pageRangeWithSnapshots = 
									from page in startOffset
									select new DbPageBlobObject.PageRangeWithSnapshot(Math.Max(page.StartOffset, blobRegion.Offset), Math.Min(page.EndOffset - (long)1, offset1), page.SnapshotCount);
							}
							else
							{
								IQueryable<CurrentPage> currentPages = 
									from page in dbContext.CurrentPages
									where (page.AccountName == this._blob.AccountName) && (page.ContainerName == this._blob.ContainerName) && (page.BlobName == this._blob.BlobName) && (page.VersionTimestamp == this._blob.VersionTimestamp)
									select page;
								currentPages = 
									from page in currentPages
									where page.StartOffset >= blobRegion.Offset && page.StartOffset < offset1 || page.EndOffset > blobRegion.Offset && page.EndOffset <= offset1
									select page;
								pageRangeWithSnapshots = 
									from page in currentPages
									select new DbPageBlobObject.PageRangeWithSnapshot(Math.Max(page.StartOffset, blobRegion.Offset), Math.Min(page.EndOffset - (long)1, offset1), page.SnapshotCount);
							}
							List<PageRange> pageRanges = new List<PageRange>();
							IEnumerable<PageRange> pageRanges1 = DbPageBlobObject.CoalescePageRanges(DbPageBlobObject.MergePageRanges(pageRangeWithSnapshot, pageRangeWithSnapshots), true);
							if (maxPageRanges != 0)
							{
								pageRanges1 = pageRanges1.Take<PageRange>(maxPageRanges + 1);
							}
							context.ResultData = new DbPageBlobObject.DbPageRangeCollection(pageRanges1, maxPageRanges);
							this._blob = pageBlob;
							this.LeaseInfo = blobLeaseInfo;
							transactionScope.Complete();
							goto Label0;
						}
						throw new InvalidBlobRegionException(pageBlob.MaxSize);
					}
				Label0:
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.GetBlockListImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute<NoResults>(asyncResult);
		}

		private static IEnumerable<PageRange> MergePageRanges(IEnumerable<DbPageBlobObject.PageRangeWithSnapshot> pastPageRanges, IEnumerable<DbPageBlobObject.PageRangeWithSnapshot> futurePageRanges)
		{
			DbPageBlobObject.PageRangeWithSnapshot current = null;
			DbPageBlobObject.PageRangeWithSnapshot pageRangeWithSnapshot = null;
			long pageEnd = (long)-1;
			IEnumerator<DbPageBlobObject.PageRangeWithSnapshot> enumerator = pastPageRanges.GetEnumerator();
			IEnumerator<DbPageBlobObject.PageRangeWithSnapshot> enumerator1 = futurePageRanges.GetEnumerator();
			bool flag = enumerator.MoveNext();
			bool flag1 = enumerator1.MoveNext();
			while (flag || flag1)
			{
				if (flag)
				{
					current = enumerator.Current;
				}
				if (flag1)
				{
					pageRangeWithSnapshot = enumerator1.Current;
				}
				if (flag && !flag1)
				{
					PageRange pageRange = new PageRange(Math.Max(pageEnd + (long)1, current.PageStart), current.PageEnd, true);
					pageEnd = pageRange.PageEnd;
					yield return pageRange;
				}
				else if (flag || !flag1)
				{
					if (pageEnd < current.PageStart - (long)1 && pageEnd < pageRangeWithSnapshot.PageStart - (long)1)
					{
						pageEnd = Math.Min(current.PageStart, pageRangeWithSnapshot.PageStart) - (long)1;
					}
					if (current.PageStart - (long)1 <= pageEnd && pageRangeWithSnapshot.PageStart - (long)1 <= pageEnd)
					{
						PageRange pageRange1 = new PageRange(pageEnd + (long)1, Math.Min(current.PageEnd, pageRangeWithSnapshot.PageEnd));
						pageEnd = pageRange1.PageEnd;
						if (current.SnapshotCount != pageRangeWithSnapshot.SnapshotCount)
						{
							yield return pageRange1;
						}
					}
					else if (current.PageStart - (long)1 > pageEnd)
					{
						PageRange pageRange2 = new PageRange(pageEnd + (long)1, Math.Min(pageRangeWithSnapshot.PageEnd, current.PageStart - (long)1));
						pageEnd = pageRange2.PageEnd;
						yield return pageRange2;
					}
					else
					{
						PageRange pageRange3 = new PageRange(pageEnd + (long)1, Math.Min(current.PageEnd, pageRangeWithSnapshot.PageStart - (long)1), true);
						pageEnd = pageRange3.PageEnd;
						yield return pageRange3;
					}
				}
				else
				{
					PageRange pageRange4 = new PageRange(Math.Max(pageEnd + (long)1, pageRangeWithSnapshot.PageStart), pageRangeWithSnapshot.PageEnd);
					pageEnd = pageRange4.PageEnd;
					yield return pageRange4;
				}
				if (flag && pageEnd >= current.PageEnd)
				{
					flag = enumerator.MoveNext();
				}
				if (!flag1 || pageEnd < pageRangeWithSnapshot.PageEnd)
				{
					continue;
				}
				flag1 = enumerator1.MoveNext();
			}
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.BeginClearPage(IBlobRegion blobRegion, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbPageBlobObject.ClearPage", callback, state);
			asyncIteratorContext.Begin(this.ClearPageImpl(blobRegion, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = new AsyncIteratorContext<IPageRangeCollection>("DbPageBlobObject.GetPageRangeList", callback, state);
			asyncIteratorContext.Begin(this.GetPageRangeListImpl(blobRegion, condition, maxPageRanges, asyncIteratorContext));
			return asyncIteratorContext;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.BeginGetPageRangeList(IBlobRegion blobRegion, BlobPropertyNames additionalPropertyNames, IBlobObjectCondition condition, int maxPageRanges, DateTime? prevSnapshotTimestamp, bool isRangeCompressed, bool skipClearPages, AsyncCallback callback, object state)
		{
			if (!prevSnapshotTimestamp.HasValue)
			{
				AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = new AsyncIteratorContext<IPageRangeCollection>("DbPageBlobObject.GetPageRangeList", callback, state);
				asyncIteratorContext.Begin(this.GetPageRangeListImpl(blobRegion, condition, maxPageRanges, asyncIteratorContext));
				return asyncIteratorContext;
			}
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext1 = new AsyncIteratorContext<IPageRangeCollection>("DbPageBlobObject.GetPageRangeList", callback, state);
			asyncIteratorContext1.Begin(this.GetPageRangeListImpl(blobRegion, condition, maxPageRanges, prevSnapshotTimestamp, isRangeCompressed, skipClearPages, asyncIteratorContext1));
			return asyncIteratorContext1;
		}

		IAsyncResult Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.BeginPutPage(IBlobRegion blobRegion, Stream inputStream, CrcReaderStream crcReaderStream, IBlobObjectCondition condition, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("DbPageBlobObject.PutPage", callback, state);
			asyncIteratorContext.Begin(this.PutPageImpl(blobRegion, inputStream, condition, asyncIteratorContext));
			return asyncIteratorContext;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.EndClearPage(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		IPageRangeCollection Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.EndGetPageRangeList(IAsyncResult asyncResult)
		{
			Exception exception;
			AsyncIteratorContext<IPageRangeCollection> asyncIteratorContext = (AsyncIteratorContext<IPageRangeCollection>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
		}

		void Microsoft.Cis.Services.Nephos.Common.Storage.IIndexBlobObject.EndPutPage(IAsyncResult asyncResult)
		{
			Exception exception;
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		private IEnumerator<IAsyncResult> PutBlobImpl(string contentType, long contentLength, long? maxBlobSize, bool is8TBPageBlobAllowed, byte[] serviceMetadata, byte[] applicationMetadata, Stream inputStream, byte[] contentMD5, ISequenceNumberUpdate sequenceNumberUpdate, OverwriteOption overwriteOption, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				StorageStampHelpers.ValidatePutBlobArguments(this, contentLength, maxBlobSize, applicationMetadata, contentMD5, sequenceNumberUpdate, overwriteOption, condition, true, is8TBPageBlobAllowed);
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						PageBlob pageBlob = base.TryLoadPageBlob(dbContext);
						this.ThrowIfIncrementalRoot(pageBlob);
						DateTime utcNow = DateTime.UtcNow;
						bool flag = false;
						if (pageBlob != null)
						{
							if (overwriteOption == OverwriteOption.CreateNewOnly)
							{
								throw new BlobAlreadyExistsException();
							}
							DbBlobObject.CheckCopyState(pageBlob);
							flag = DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, new BlobLeaseInfo(pageBlob, utcNow), condition, null, true);
							CurrentPage currentPage = new CurrentPage()
							{
								AccountName = this._blob.AccountName,
								ContainerName = this._blob.ContainerName,
								BlobName = this._blob.BlobName,
								VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
								StartOffset = (long)0,
								EndOffset = 9223372036854775807L
							};
							dbContext.CurrentPages.Attach(currentPage);
							dbContext.CurrentPages.DeleteOnSubmit(currentPage);
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
							pageBlob = new PageBlob()
							{
								AccountName = this._blob.AccountName,
								ContainerName = this._blob.ContainerName,
								BlobName = this._blob.BlobName,
								VersionTimestamp = DateTime.SpecifyKind(DateTime.MaxValue, DateTimeKind.Utc),
								SequenceNumber = new long?((long)0),
								FileName = Guid.NewGuid().ToString()
							};
							dbContext.Blobs.InsertOnSubmit(pageBlob);
						}
						pageBlob.ContentType = contentType ?? "application/octet-stream";
						pageBlob.ContentMD5 = contentMD5;
						pageBlob.ServiceMetadata = serviceMetadata;
						pageBlob.Metadata = applicationMetadata;
						pageBlob.MaxSize = new long?(maxBlobSize.Value);
						pageBlob.SnapshotCount = 0;
						pageBlob.GenerationId = Guid.NewGuid().ToString();
						base.ResetBlobLeaseToAvailable(pageBlob, flag);
						if (sequenceNumberUpdate != null)
						{
							DbBlobObject.SetSequenceNumber(pageBlob, sequenceNumberUpdate);
						}
						this.CopyStream(inputStream, pageBlob.FileName);
						dbContext.SubmitChanges();
						dbContext.Refresh(RefreshMode.OverwriteCurrentValues, pageBlob);
						transactionScope.Complete();
						this._blob = pageBlob;
						this.LeaseInfo = new BlobLeaseInfo(pageBlob, utcNow);
					}
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.PutBlob"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> PutPageImpl(IBlobRegion blobRegion, Stream inputStream, IBlobObjectCondition condition, AsyncIteratorContext<NoResults> context)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute((TimeSpan param0) => {
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					base.LoadContainer(dbContext);
					PageBlob pageBlob = base.LoadPageBlob(dbContext);
					this.ThrowIfIncrementalRoot(pageBlob);
					bool flag = false;
					PropertyChangedEventHandler propertyChangedEventHandler = (object sender, PropertyChangedEventArgs eventArgs) => flag = true;
					pageBlob.PropertyChanged += propertyChangedEventHandler;
					BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
					DbBlobObject.CheckCopyState(pageBlob);
					bool flag1 = DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, true);
					long offset = blobRegion.Offset;
					long? maxSize = pageBlob.MaxSize;
					if ((offset < maxSize.GetValueOrDefault() ? true : !maxSize.HasValue))
					{
						long num = blobRegion.Offset + blobRegion.Length;
						long? nullable = pageBlob.MaxSize;
						if ((num <= nullable.GetValueOrDefault() ? true : !nullable.HasValue))
						{
							CurrentPage currentPage = new CurrentPage()
							{
								AccountName = this._blob.AccountName,
								ContainerName = this._blob.ContainerName,
								BlobName = this._blob.BlobName,
								VersionTimestamp = this._blob.VersionTimestamp,
								StartOffset = blobRegion.Offset,
								EndOffset = blobRegion.Offset + blobRegion.Length,
								SnapshotCount = pageBlob.SnapshotCount
							};
							byte[] byteArrayFromStream = DbStorageHelper.GetByteArrayFromStream(inputStream, blobRegion.Length, true, false);
							lock (DbPageBlobObject.m_writeFileLock)
							{
								using (FileStream fileStream = new FileStream(DbPageBlobObject.GetFilePath(pageBlob.FileName), FileMode.Open, FileAccess.Write))
								{
									fileStream.Seek(blobRegion.Offset, SeekOrigin.Begin);
									fileStream.Write(byteArrayFromStream, 0, (int)byteArrayFromStream.Length);
								}
								base.ResetBlobLeaseToAvailable(pageBlob, flag1);
								using (TransactionScope transactionScope = null)
								{
									if (flag)
									{
										transactionScope = new TransactionScope();
										dbContext.SubmitChanges();
									}
									dbContext.CurrentPages.InsertOnSubmit(currentPage);
									dbContext.SubmitChanges();
									if (transactionScope != null)
									{
										transactionScope.Complete();
									}
								}
								dbContext.Refresh(RefreshMode.OverwriteCurrentValues, pageBlob);
							}
							pageBlob.PropertyChanged -= propertyChangedEventHandler;
							this._blob = pageBlob;
							blobLeaseInfo.SetBlob(pageBlob, blobLeaseInfo.LeaseInfoValidAt);
							this.LeaseInfo = blobLeaseInfo;
							return;
						}
					}
					throw new PageRangeInvalidException();
				}
			}, base.Timeout, context.GetResumeCallback(), context.GetResumeState("DbPageBlobObject.PutPageImpl"));
			yield return asyncResult;
			this._storageManager.AsyncProcessor.EndExecute(asyncResult);
		}

		private IEnumerator<IAsyncResult> SnapshotBlobImpl(byte[] metadata, IBlobObjectCondition condition, AsyncIteratorContext<DateTime> context, bool throwIfRootBlob)
		{
			IAsyncResult asyncResult = this._storageManager.AsyncProcessor.BeginExecute<DateTime>((TimeSpan param0) => {
				DateTime value;
				StorageStampHelpers.ValidateApplicationMetadata(metadata);
				using (TransactionScope transactionScope = new TransactionScope())
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						base.LoadContainer(dbContext);
						PageBlob pageBlob = base.LoadPageBlob(dbContext);
						if (throwIfRootBlob)
						{
							this.ThrowIfIncrementalRoot(pageBlob);
						}
						BlobLeaseInfo blobLeaseInfo = new BlobLeaseInfo(pageBlob, DateTime.UtcNow);
						DbBlobObject.CheckConditionsAndReturnResetRequired(pageBlob, blobLeaseInfo, condition, null, false);
						DateTime? nullable = null;
						DateTime? nullable1 = null;
						string str = Guid.NewGuid().ToString();
						File.Copy(DbPageBlobObject.GetFilePath(pageBlob.FileName), DbPageBlobObject.GetFilePath(str), true);
						dbContext.SnapshotPageBlob(pageBlob.AccountName, pageBlob.ContainerName, pageBlob.BlobName, metadata, str, new int?(pageBlob.SnapshotCount), ref nullable, ref nullable1);
						transactionScope.Complete();
						nullable = new DateTime?(DateTime.SpecifyKind(nullable.Value, DateTimeKind.Utc));
						this._blob = pageBlob;
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

		private void ThrowIfIncrementalCopy(PageBlob blob)
		{
			if (blob != null)
			{
				bool? isIncrementalCopy = blob.IsIncrementalCopy;
				if ((!isIncrementalCopy.GetValueOrDefault() ? false : isIncrementalCopy.HasValue))
				{
					throw new OperationNotAllowedOnIncrementalCopyBlobException();
				}
			}
		}

		private void ThrowIfIncrementalRoot(PageBlob blob)
		{
			if (blob != null && blob.IsIncrementalRoot)
			{
				throw new OperationNotAllowedOnIncrementalCopyBlobException();
			}
		}

		private string ToHexString(byte[] bytes)
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

		private class DbPageRangeCollection : IPageRangeCollection, IEnumerable<IPageRange>, IEnumerable, IDisposable
		{
			private List<IPageRange> m_pageRanges;

			private int m_maxPageRanges;

			bool Microsoft.Cis.Services.Nephos.Common.Storage.IPageRangeCollection.HasMoreRows
			{
				get
				{
					if (this.m_maxPageRanges == 0)
					{
						return false;
					}
					return this.m_pageRanges.Count > this.m_maxPageRanges;
				}
			}

			long Microsoft.Cis.Services.Nephos.Common.Storage.IPageRangeCollection.NextPageStart
			{
				get
				{
					if (!((IPageRangeCollection)this).HasMoreRows)
					{
						throw new InvalidOperationException();
					}
					return this.m_pageRanges[this.m_pageRanges.Count - 1].PageStart;
				}
			}

			int Microsoft.Cis.Services.Nephos.Common.Storage.IPageRangeCollection.PageRangeCount
			{
				get
				{
					return this.m_pageRanges.Count;
				}
			}

			public DbPageRangeCollection(IEnumerable<PageRange> pageRanges, int maxPageRanges)
			{
				this.m_pageRanges = pageRanges.Cast<IPageRange>().ToList<IPageRange>();
				this.m_maxPageRanges = maxPageRanges;
			}

			IEnumerator<IPageRange> System.Collections.Generic.IEnumerable<Microsoft.Cis.Services.Nephos.Common.Storage.IPageRange>.GetEnumerator()
			{
				return this.m_pageRanges.GetEnumerator();
			}

			IEnumerator System.Collections.IEnumerable.GetEnumerator()
			{
				return this.m_pageRanges.GetEnumerator();
			}

			void System.IDisposable.Dispose()
			{
			}
		}

		internal class PageRangeWithSnapshot : PageRange
		{
			internal int SnapshotCount;

			public PageRangeWithSnapshot(long startOffset, long endOffset, int snapshotCount) : base(startOffset, endOffset)
			{
				this.SnapshotCount = snapshotCount;
			}
		}
	}
}