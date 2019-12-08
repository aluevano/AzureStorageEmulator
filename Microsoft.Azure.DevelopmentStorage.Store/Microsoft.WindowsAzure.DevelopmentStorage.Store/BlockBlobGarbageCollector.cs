using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Timers;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlockBlobGarbageCollector
	{
		private const double deleteOrphanedFilesInterval = 900000;

		private const string DELETE_ORPHAN_FILES_INTERVAL_MILLIS_ENVIRON_VARIABLE = "DEVSTORE_DELETE_FILES_MILLIS";

		private const string SYNC_ITEMS_COUNT_ENVIRON_VARIABLE = "DEVSTORE_DEFRAGMENT_BLOCK_SYNC_COUNT";

		private const double defragmentFileInterval = 900000;

		private const string DEFRAGMENT_FILES_INTERVAL_MILLIS_ENVIRON_VARIABLE = "DEVSTORE_DEFRAGMENT_FILES_MILLIS";

		private const double timeStampDifferenceInMinutes = 20;

		private const int maxNumberOfGCWorkers = 2;

		private const double defragmentLimitFactor = 2;

		private const int maxDefragRetryAttempts = 5;

		private static bool isInitialized;

		private static System.Timers.Timer gcTimer;

		private static System.Timers.Timer defragmentTimer;

		private static Dictionary<string, BlockBlobMetaInfo> toDefragmentDict;

		private static Queue<BlockBlobMetaInfo> defragWorkItems;

		private static object defragLock;

		private static bool shouldInitializeWorkers;

		static BlockBlobGarbageCollector()
		{
			BlockBlobGarbageCollector.isInitialized = false;
			BlockBlobGarbageCollector.toDefragmentDict = new Dictionary<string, BlockBlobMetaInfo>();
			BlockBlobGarbageCollector.defragWorkItems = new Queue<BlockBlobMetaInfo>();
			BlockBlobGarbageCollector.defragLock = new object();
			BlockBlobGarbageCollector.shouldInitializeWorkers = true;
		}

		public BlockBlobGarbageCollector()
		{
		}

		public static void DefragmentBlobFiles(object source, ElapsedEventArgs e)
		{
			string str;
			string str1;
			try
			{
				using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
				{
					IQueryable<BlockBlobMetaInfo> blocksData = 
						from b in dbContext.BlocksData
						group b by new { AccountName = b.AccountName, BlobName = b.BlobName, ContainerName = b.ContainerName, DirectoryPath = b.BlockBlob.DirectoryPath } into blobBlocks
						select new BlockBlobMetaInfo(blobBlocks.Key.AccountName, blobBlocks.Key.ContainerName, blobBlocks.Key.BlobName, blobBlocks.Key.DirectoryPath)
						{
							BlobSize = blobBlocks.Sum<BlockData>((BlockData x) => x.Length.GetValueOrDefault((long)0))
						};
					foreach (BlockBlobMetaInfo blocksDatum in blocksData)
					{
						BlockBlobDataManager.GetDataFiles(blocksDatum, out str, out str1);
						IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
						object[] blobName = new object[] { blocksDatum.BlobName, blocksDatum.ContainerName, blocksDatum.BlobDirectory, str, str1, blocksDatum.BlobSize };
						infoDebug.Log("BlockBlob: DefragmentBlobFiles BlobInfo Name {0}, ContainerName {1}, Directory {2}, ROFile {3}, RWFile {4}, Size{5}", blobName);
						long length = (long)0;
						FileInfo fileInfo = null;
						if (!string.IsNullOrEmpty(str))
						{
							fileInfo = new FileInfo(str);
						}
						FileInfo fileInfo1 = null;
						if (!string.IsNullOrEmpty(str1))
						{
							fileInfo1 = new FileInfo(str1);
						}
						if (fileInfo1 != null && fileInfo1.Exists)
						{
							IStringDataEventStream stringDataEventStream = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
							object[] objArray = new object[] { fileInfo1.Length };
							stringDataEventStream.Log("BlockBlob: DefragmentBlobFiles RWFileLength {0}", objArray);
							length += fileInfo1.Length;
						}
						if (fileInfo != null && fileInfo.Exists)
						{
							IStringDataEventStream infoDebug1 = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
							object[] length1 = new object[] { fileInfo.Length };
							infoDebug1.Log("BlockBlob: DefragmentBlobFiles ROFileLength {0}", length1);
							length += fileInfo.Length;
						}
						if ((double)length <= (double)blocksDatum.BlobSize * 2)
						{
							continue;
						}
						Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: DefragmentBlobFiles FileSize {0}", new object[] { length });
						string key = blocksDatum.GetKey();
						lock (BlockBlobGarbageCollector.defragLock)
						{
							if (!BlockBlobGarbageCollector.toDefragmentDict.ContainsKey(key))
							{
								BlockBlobGarbageCollector.toDefragmentDict.Add(key, blocksDatum);
								BlockBlobGarbageCollector.defragWorkItems.Enqueue(blocksDatum);
								Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: DefragmentBlobFiles QueuedWork");
								Monitor.Pulse(BlockBlobGarbageCollector.defragLock);
							}
						}
					}
				}
				if (BlockBlobGarbageCollector.shouldInitializeWorkers)
				{
					for (int i = 0; i < 2; i++)
					{
						ThreadPool.QueueUserWorkItem(new WaitCallback(BlockBlobGarbageCollector.DoDefragmentBlobWork));
					}
					BlockBlobGarbageCollector.shouldInitializeWorkers = false;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: DefragmentBlobFiles Exception {0}", new object[] { exception.StackTrace });
			}
			BlockBlobGarbageCollector.defragmentTimer.Interval = BlockBlobGarbageCollector.GetTimerIntervalOrDefault(false);
			BlockBlobGarbageCollector.defragmentTimer.Start();
		}

		public static void DeleteOrphanedFiles(object source, ElapsedEventArgs e)
		{
			try
			{
				try
				{
					using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
					{
						BlockBlobDataManager.CurrentBlobCount = dbContext.Blobs.OfType<BlockBlob>().Count<BlockBlob>();
						if (dbContext.Blobs.Any<Blob>((Blob blob) => blob is BlockBlob || blob is PageBlob))
						{
							DateTime utcNow = DateTime.UtcNow;
							DateTime dateTime = utcNow.Subtract(TimeSpan.FromMinutes(20));
							HashSet<string> strs1 = new HashSet<string>((
								from topDir in BlockBlobDataManager.BlobDataTopLevelDirectories
								where Directory.Exists(topDir)
								select topDir).SelectMany<string, string>((string topDir) => {
								IEnumerable<string> strs;
								try
								{
									strs = 
										from dir in Directory.EnumerateDirectories(topDir)
										where Directory.GetCreationTimeUtc(dir) < dateTime
										select dir;
								}
								catch (Exception exception)
								{
									Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: Listing Directories under {0} failed. Exception: {1}", new object[] { topDir, exception.ToString() });
									strs = Enumerable.Empty<string>();
								}
								return strs;
							}), StringComparer.InvariantCultureIgnoreCase);
							IQueryable<string> strs2 = 
								from blob in dbContext.Blobs.OfType<BlockBlob>()
								select blob.DirectoryPath;
							strs1.ExceptWith(strs2);
							foreach (string str in strs1)
							{
								try
								{
									Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: Deleting Directory {0}", new object[] { str });
									Directory.Delete(str, true);
									BlockBlobDataManager.RemoveEntryIfExists(str);
								}
								catch (Exception exception2)
								{
									Exception exception1 = exception2;
									Logger<INormalAndDebugLogger>.Instance.Error.Log("DeleteOrphanedFiles: Unable to delete directory {0} with Exception {1}", new object[] { str, exception1 });
								}
							}
							HashSet<string> strs3 = new HashSet<string>(
								from dir in Directory.EnumerateFiles(DevelopmentStorageDbDataContext.PageBlobRoot)
								where Directory.GetCreationTimeUtc(dir) < dateTime
								select dir, StringComparer.InvariantCultureIgnoreCase);
							IQueryable<string> strs4 = 
								from blob in dbContext.Blobs.OfType<PageBlob>()
								where blob.FileName != null
								select blob.FileName into fileName
								select DbPageBlobObject.GetFilePath(fileName);
							strs3.ExceptWith(strs4);
							foreach (string str1 in strs3)
							{
								try
								{
									string filePath = DbPageBlobObject.GetFilePath(str1);
									Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("PageBlob: Deleting File {0}", new object[] { filePath });
									File.Delete(filePath);
								}
								catch (Exception exception4)
								{
									Exception exception3 = exception4;
									Logger<INormalAndDebugLogger>.Instance.Error.Log("DeleteOrphanedFiles: Unable to delete page blob files with Exception {0}", new object[] { exception3 });
								}
							}
						}
					}
				}
				catch (Exception exception6)
				{
					Exception exception5 = exception6;
					Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: DeleteOrphanedFiles: Exception {0}", new object[] { exception5 });
				}
			}
			finally
			{
				BlockBlobGarbageCollector.gcTimer.Interval = BlockBlobGarbageCollector.GetTimerIntervalOrDefault(true);
				BlockBlobGarbageCollector.gcTimer.Start();
			}
		}

		private static void DoDefragmentBlobWork(object info)
		{
			string str;
			bool flag;
			long num;
			string str1;
			while (true)
			{
				BlockBlobMetaInfo blockBlobMetaInfo = null;
				lock (BlockBlobGarbageCollector.defragLock)
				{
					if (BlockBlobGarbageCollector.defragWorkItems.Count == 0)
					{
						Monitor.Wait(BlockBlobGarbageCollector.defragLock);
					}
					blockBlobMetaInfo = BlockBlobGarbageCollector.defragWorkItems.Dequeue();
				}
				IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
				object[] objArray = new object[] { blockBlobMetaInfo.ToString() };
				infoDebug.Log("BlockBlob: DoDefragmentBlobWork WorkItem! {0}", objArray);
				try
				{
					BlockBlobDataManager.GetDataFiles(blockBlobMetaInfo, out str1, out str);
					Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: DoDefragmentBlobWork readOnlyFile {0}, readWriteFile {1}", new object[] { str1, str });
					if (string.IsNullOrEmpty(str1))
					{
						BlockBlobDataManager.SealCurrentWriteFile(blockBlobMetaInfo, -1);
					}
					BlockBlobDataManager.GetDataFiles(blockBlobMetaInfo, out str1, out str);
					Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: DoDefragmentBlobWork NEW readOnlyFile {0}, readWriteFile {1}", new object[] { str1, str });
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: DoDefragmentBlobWork Exception {0}", new object[] { exception.StackTrace });
					BlockBlobGarbageCollector.RemoveWorkFromProcessingDictionary(blockBlobMetaInfo);
					continue;
				}
				int num1 = 0;
				do
				{
				Label0:
					if (num1 >= 5)
					{
						break;
					}
					flag = true;
					int maxItemsToSync = BlockBlobGarbageCollector.GetMaxItemsToSync();
					try
					{
						using (DevelopmentStorageDbDataContext dbContext = DevelopmentStorageDbDataContext.GetDbContext())
						{
							IQueryable<BlockData> blockDatas = (
								from b in dbContext.BlocksData
								where (b.ContainerName == blockBlobMetaInfo.ContainerName) && (b.BlobName == blockBlobMetaInfo.BlobName) && (b.FilePath == str1)
								select b).Take<BlockData>(maxItemsToSync);
							foreach (BlockData nullable in blockDatas)
							{
								flag = false;
								byte[] numArray = null;
								using (FileStream fileStream = new FileStream(str1, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
								{
									fileStream.Seek(nullable.StartOffset.Value, SeekOrigin.Begin);
									numArray = new byte[checked((IntPtr)nullable.Length.Value)];
									fileStream.Read(numArray, 0, (int)numArray.Length);
								}
								string file = BlockBlobDataManager.WriteBytesToFile(blockBlobMetaInfo, numArray, out num);
								nullable.FilePath = file;
								nullable.StartOffset = new long?(num);
							}
							dbContext.SubmitChanges(ConflictMode.ContinueOnConflict);
						}
					}
					catch (Exception exception3)
					{
						Exception exception2 = exception3;
						num1++;
						IStringDataEventStream error = Logger<INormalAndDebugLogger>.Instance.Error;
						object[] stackTrace = new object[] { exception2.StackTrace, num1 };
						error.Log("BlockBlob: DoDefragmentBlobWork Exception {0}, Attempt {1}", stackTrace);
						goto Label0;
					}
				}
				while (!flag);
				if (num1 < 5)
				{
					try
					{
						BlockBlobDataManager.ResetReadOnlyFileToNull(blockBlobMetaInfo);
					}
					catch (Exception exception5)
					{
						Exception exception4 = exception5;
						Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: DoDefragmentBlobWork Exception while setting up rofile to null {0} ", new object[] { exception4.StackTrace });
					}
				}
				BlockBlobGarbageCollector.RemoveWorkFromProcessingDictionary(blockBlobMetaInfo);
			}
		}

		private static int GetMaxItemsToSync()
		{
			int num = 1;
			try
			{
				int num1 = int.Parse(Environment.GetEnvironmentVariable("DEVSTORE_DEFRAGMENT_BLOCK_SYNC_COUNT", EnvironmentVariableTarget.User));
				if (num1 > 0)
				{
					num = num1;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: GetMaxItemsToSync failed.Exception {0}", new object[] { exception.StackTrace });
			}
			return num;
		}

		private static double GetTimerIntervalOrDefault(bool isGC)
		{
			double num = 900000;
			try
			{
				if (!isGC)
				{
					num = 900000;
					double num1 = double.Parse(Environment.GetEnvironmentVariable("DEVSTORE_DEFRAGMENT_FILES_MILLIS", EnvironmentVariableTarget.User));
					if (num1 >= 30000)
					{
						num = num1;
					}
				}
				else
				{
					num = 900000;
					double num2 = double.Parse(Environment.GetEnvironmentVariable("DEVSTORE_DELETE_FILES_MILLIS", EnvironmentVariableTarget.User));
					if (num2 >= 30000)
					{
						num = num2;
					}
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
				object[] objArray = new object[] { isGC, exception.StackTrace };
				infoDebug.Log("BlockBlob: Load Interval failed. IsGC: {0}, Exception {1}", objArray);
			}
			return num;
		}

		public static void Initialize()
		{
			if (!BlockBlobGarbageCollector.isInitialized)
			{
				lock (typeof(BlockBlobGarbageCollector))
				{
					if (!BlockBlobGarbageCollector.isInitialized)
					{
						BlockBlobGarbageCollector.gcTimer = new System.Timers.Timer()
						{
							Enabled = false,
							AutoReset = false
						};
						BlockBlobGarbageCollector.gcTimer.Elapsed += new ElapsedEventHandler(BlockBlobGarbageCollector.DeleteOrphanedFiles);
						BlockBlobGarbageCollector.gcTimer.Interval = BlockBlobGarbageCollector.GetTimerIntervalOrDefault(true);
						BlockBlobGarbageCollector.gcTimer.Start();
						BlockBlobGarbageCollector.defragmentTimer = new System.Timers.Timer()
						{
							Enabled = false,
							AutoReset = false
						};
						BlockBlobGarbageCollector.defragmentTimer.Elapsed += new ElapsedEventHandler(BlockBlobGarbageCollector.DefragmentBlobFiles);
						BlockBlobGarbageCollector.defragmentTimer.Interval = BlockBlobGarbageCollector.GetTimerIntervalOrDefault(false);
						BlockBlobGarbageCollector.defragmentTimer.Start();
					}
					BlockBlobGarbageCollector.isInitialized = true;
				}
			}
		}

		private static void RemoveWorkFromProcessingDictionary(BlockBlobMetaInfo info)
		{
			lock (BlockBlobGarbageCollector.defragLock)
			{
				string key = info.GetKey();
				BlockBlobGarbageCollector.toDefragmentDict.Remove(key);
			}
		}
	}
}