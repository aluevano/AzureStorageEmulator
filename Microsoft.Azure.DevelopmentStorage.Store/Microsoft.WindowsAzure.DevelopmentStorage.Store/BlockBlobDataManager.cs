using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlockBlobDataManager
	{
		private const int maxNumberOfBlobsPerDirectory = 500;

		private static string[] fileNames;

		private static string[] blobDataDirectories;

		private static volatile int currentBlobCount;

		private static Dictionary<string, DevStoreBlockBlobFileParameters> blobInfoMap;

		public static string[] BlobDataTopLevelDirectories
		{
			get
			{
				return BlockBlobDataManager.blobDataDirectories;
			}
			private set
			{
				BlockBlobDataManager.blobDataDirectories = value;
			}
		}

		public static int CurrentBlobCount
		{
			get
			{
				return BlockBlobDataManager.currentBlobCount;
			}
			set
			{
				BlockBlobDataManager.currentBlobCount = value;
			}
		}

		static BlockBlobDataManager()
		{
			BlockBlobDataManager.fileNames = new string[] { "1", "2" };
			string[] strArrays = new string[] { Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "1"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "2"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "3"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "4"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "5"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "6"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "7"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "8"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "9"), Path.Combine(DevelopmentStorageDbDataContext.BlockBlobRoot, "10") };
			BlockBlobDataManager.blobDataDirectories = strArrays;
			BlockBlobDataManager.blobInfoMap = new Dictionary<string, DevStoreBlockBlobFileParameters>();
		}

		public BlockBlobDataManager()
		{
		}

		private static DevStoreBlockBlobFileParameters AddInfoIfNotExists(BlockBlobMetaInfo blobInfo)
		{
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter;
			string key = blobInfo.GetKey();
			if (!BlockBlobDataManager.blobInfoMap.TryGetValue(key, out devStoreBlockBlobFileParameter))
			{
				lock (BlockBlobDataManager.blobInfoMap)
				{
					if (!BlockBlobDataManager.blobInfoMap.TryGetValue(key, out devStoreBlockBlobFileParameter))
					{
						devStoreBlockBlobFileParameter = new DevStoreBlockBlobFileParameters();
						BlockBlobDataManager.InitializeBlockBlobFileAttributes(blobInfo.BlobDirectory, devStoreBlockBlobFileParameter);
						BlockBlobDataManager.blobInfoMap.Add(key, devStoreBlockBlobFileParameter);
					}
				}
			}
			return devStoreBlockBlobFileParameter;
		}

		public static string CreateUniqueDirectory(string parentDirectory)
		{
			string lowerInvariant;
			try
			{
				Guid guid = Guid.NewGuid();
				DirectoryInfo directoryInfo = Directory.CreateDirectory(Path.Combine(parentDirectory, guid.ToString()));
				lowerInvariant = directoryInfo.FullName.ToLowerInvariant();
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: CreateUniqueDirectory to create unique directory failed! Exception: {0}", new object[] { exception.StackTrace });
				SqlExceptionManager.ReThrowException(exception);
				throw;
			}
			return lowerInvariant;
		}

		public static string CreateUniqueDirectoryLoadBalanced()
		{
			string str = BlockBlobDataManager.blobDataDirectories[BlockBlobDataManager.CurrentBlobCount / 500 % (int)BlockBlobDataManager.blobDataDirectories.Length];
			Directory.CreateDirectory(str);
			return BlockBlobDataManager.CreateUniqueDirectory(str);
		}

		public static void GetDataFiles(BlockBlobMetaInfo blobInfo, out string readOnlyFile, out string readWriteFile)
		{
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter = BlockBlobDataManager.AddInfoIfNotExists(blobInfo);
			devStoreBlockBlobFileParameter.AccessReadWriteLock.EnterReadLock();
			readWriteFile = devStoreBlockBlobFileParameter.ReadWriteFile;
			readOnlyFile = devStoreBlockBlobFileParameter.ReadOnlyFile;
			devStoreBlockBlobFileParameter.AccessReadWriteLock.ExitReadLock();
		}

		private static void InitializeBlockBlobFileAttributes(string directory, DevStoreBlockBlobFileParameters info)
		{
			if (!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			info.DirectoryName = directory;
			DateTime lastWriteTimeUtc = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
			string[] strArrays = BlockBlobDataManager.fileNames;
			for (int i = 0; i < (int)strArrays.Length; i++)
			{
				string str = strArrays[i];
				FileInfo fileInfo = new FileInfo(Path.Combine(directory, str));
				if (info.ReadWriteFile == null)
				{
					info.ReadWriteFile = fileInfo.FullName;
					info.LastAllottedChunkEnd = (long)0;
				}
				if (fileInfo.Exists && fileInfo.LastWriteTimeUtc > lastWriteTimeUtc)
				{
					lastWriteTimeUtc = fileInfo.LastWriteTimeUtc;
					if (string.Compare(info.ReadWriteFile, fileInfo.FullName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						info.ReadOnlyFile = info.ReadWriteFile;
					}
					info.ReadWriteFile = fileInfo.FullName;
					info.LastAllottedChunkEnd = fileInfo.Length;
				}
			}
		}

		private static void InitializeNewReadWriteFile(DevStoreBlockBlobFileParameters info)
		{
			info.ReadOnlyFile = info.ReadWriteFile;
			int num = 0;
			while (num < (int)BlockBlobDataManager.fileNames.Length)
			{
				if (!info.ReadWriteFile.EndsWith(BlockBlobDataManager.fileNames[num], StringComparison.OrdinalIgnoreCase))
				{
					num++;
				}
				else
				{
					info.ReadWriteFile = Path.Combine(info.DirectoryName, BlockBlobDataManager.fileNames[(num + 1) % (int)BlockBlobDataManager.fileNames.Length]);
					break;
				}
			}
			if (string.Compare(info.ReadOnlyFile, info.ReadWriteFile, StringComparison.OrdinalIgnoreCase) == 0)
			{
				Logger<INormalAndDebugLogger>.Instance.Error.Log("R/W file and R/O file are the same. Something is wrong. RWFILE{0}", new object[] { info.ReadWriteFile });
				string str = Path.Combine(info.DirectoryName, BlockBlobDataManager.fileNames[0]);
				string str1 = str;
				info.ReadWriteFile = str;
				info.ReadWriteFile = str1;
				info.ReadOnlyFile = null;
				throw new NephosAssertionException("R/W file and R/O file are the same. Something is wrong.");
			}
			info.LastAllottedChunkEnd = (long)0;
			FileInfo fileInfo = new FileInfo(info.ReadWriteFile);
			if (fileInfo.Exists)
			{
				info.LastAllottedChunkEnd = fileInfo.Length;
			}
			IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
			object[] readOnlyFile = new object[] { info.ReadOnlyFile, info.ReadWriteFile, info.LastAllottedChunkEnd };
			infoDebug.Log("BlockBlob: InitializeNewReadWriteFile ROFile {0} , RWFile {1}, NExtStartIndex {2}", readOnlyFile);
		}

		public static string MakeBlobDataCopy(BlockBlobMetaInfo blobInfo)
		{
			string str;
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter = BlockBlobDataManager.AddInfoIfNotExists(blobInfo);
			try
			{
				try
				{
					devStoreBlockBlobFileParameter.AccessReadWriteLock.EnterWriteLock();
					string str1 = BlockBlobDataManager.CreateUniqueDirectoryLoadBalanced();
					string[] files = Directory.GetFiles(devStoreBlockBlobFileParameter.DirectoryName);
					string str2 = null;
					IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
					object[] directoryName = new object[] { devStoreBlockBlobFileParameter.DirectoryName, str1 };
					infoDebug.Log("BlockBlob: MakeBlobDataCopy SrcDir {0} -> DestDir {1}", directoryName);
					if (files != null && (int)files.Length > 0)
					{
						string[] strArrays = files;
						for (int i = 0; i < (int)strArrays.Length; i++)
						{
							string str3 = strArrays[i];
							string str4 = Path.Combine(str1, Path.GetFileName(str3));
							if (string.Compare(str3, devStoreBlockBlobFileParameter.ReadWriteFile, StringComparison.OrdinalIgnoreCase) == 0)
							{
								str2 = str4;
							}
							else
							{
								Logger<INormalAndDebugLogger>.Instance.InfoDebug.Log("BlockBlob: MakeBlobDataCopy SrcFile {0} -> DestFile {1}", new object[] { str3, str4 });
								File.Copy(str3, str4);
							}
						}
						IStringDataEventStream stringDataEventStream = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
						object[] readWriteFile = new object[] { devStoreBlockBlobFileParameter.ReadWriteFile, str2 };
						stringDataEventStream.Log("BlockBlob: MakeBlobDataCopy RW FILE SrcFile {0} -> DestFile {1}", readWriteFile);
						File.Copy(devStoreBlockBlobFileParameter.ReadWriteFile, str2);
					}
					str = str1;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: MakeBlobDataCopy failed. Exception:{0}", new object[] { exception.StackTrace });
					throw;
				}
			}
			finally
			{
				devStoreBlockBlobFileParameter.AccessReadWriteLock.ExitWriteLock();
			}
			return str;
		}

		public static void RemoveEntryIfExists(string directoryPath)
		{
			lock (BlockBlobDataManager.blobInfoMap)
			{
				foreach (KeyValuePair<string, DevStoreBlockBlobFileParameters> keyValuePair in BlockBlobDataManager.blobInfoMap)
				{
					if (string.Compare(directoryPath, keyValuePair.Value.DirectoryName, StringComparison.OrdinalIgnoreCase) != 0)
					{
						continue;
					}
					BlockBlobDataManager.blobInfoMap.Remove(keyValuePair.Key);
					break;
				}
			}
		}

		public static void ResetReadOnlyFileToNull(BlockBlobMetaInfo blobInfo)
		{
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter = BlockBlobDataManager.AddInfoIfNotExists(blobInfo);
			devStoreBlockBlobFileParameter.AccessReadWriteLock.EnterReadLock();
			try
			{
				try
				{
					File.Delete(devStoreBlockBlobFileParameter.ReadOnlyFile);
					devStoreBlockBlobFileParameter.ReadOnlyFile = null;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: ResetReadOnlyFileToNull Exception {0}", new object[] { exception.StackTrace });
					throw;
				}
			}
			finally
			{
				devStoreBlockBlobFileParameter.AccessReadWriteLock.ExitReadLock();
			}
		}

		public static bool SealCurrentWriteFile(BlockBlobMetaInfo blobInfo, int timeoutInMillis)
		{
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter = BlockBlobDataManager.AddInfoIfNotExists(blobInfo);
			bool flag = false;
			if (devStoreBlockBlobFileParameter.AccessReadWriteLock.TryEnterWriteLock(timeoutInMillis))
			{
				try
				{
					BlockBlobDataManager.InitializeNewReadWriteFile(devStoreBlockBlobFileParameter);
					flag = true;
				}
				finally
				{
					devStoreBlockBlobFileParameter.AccessReadWriteLock.ExitWriteLock();
				}
			}
			return flag;
		}

		public static string WriteBytesToFile(BlockBlobMetaInfo blobInfo, byte[] content, out long startIndex)
		{
			string str;
			DevStoreBlockBlobFileParameters devStoreBlockBlobFileParameter = BlockBlobDataManager.AddInfoIfNotExists(blobInfo);
			IStringDataEventStream infoDebug = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
			object[] blobDirectory = new object[] { blobInfo.BlobDirectory, content.LongLength, blobInfo.ContainerName, blobInfo.BlobName };
			infoDebug.Log("BlockBlob: WriteBytesToFile! Directory {0}, contentLen {1} ,containerName {2}, blobName {3}", blobDirectory);
			try
			{
				try
				{
					devStoreBlockBlobFileParameter.AccessReadWriteLock.EnterReadLock();
					string readWriteFile = devStoreBlockBlobFileParameter.ReadWriteFile;
					long num = Interlocked.Add(ref devStoreBlockBlobFileParameter.LastAllottedChunkEnd, content.LongLength);
					startIndex = num - content.LongLength;
					IStringDataEventStream stringDataEventStream = Logger<INormalAndDebugLogger>.Instance.InfoDebug;
					object[] objArray = new object[] { startIndex, num };
					stringDataEventStream.Log("BlockBlob: WriteBytesToFile oldIndex {0} , newIndex {1}", objArray);
					using (FileStream fileStream = new FileStream(readWriteFile, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite))
					{
						fileStream.Seek(startIndex, SeekOrigin.Begin);
						fileStream.Write(content, 0, (int)content.Length);
					}
					str = readWriteFile;
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					Logger<INormalAndDebugLogger>.Instance.Error.Log("BlockBlob: WriteBytesToFile Exception: {0}", new object[] { exception.StackTrace });
					throw;
				}
			}
			finally
			{
				devStoreBlockBlobFileParameter.AccessReadWriteLock.ExitReadLock();
			}
			return str;
		}
	}
}