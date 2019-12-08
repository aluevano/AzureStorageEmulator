using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class CopyBlobManager
	{
		public CopyBlobManager()
		{
		}

		public static void AcceptAsycCopyJob(string copySource, bool isAzureBlob, string eTag, bool isBlockBlob, long contentLength, string accountName, string containerName, string blobName, DateTime versionTimeStamp, string copyId, DateTime? lmt, Action onSuccessfulCopy)
		{
			object[] blobHttpDownloader = new object[] { new BlobHttpDownloader(copySource, isAzureBlob, eTag, isBlockBlob, contentLength), new BlobDbUploader(accountName, containerName, blobName, versionTimeStamp, contentLength, copyId, lmt) };
			object[] objArray = blobHttpDownloader;
			ThreadPool.QueueUserWorkItem((object o) => CopyBlobManager.CopyBlobJob(o, onSuccessfulCopy), objArray);
		}

		private static void CopyBlobJob(object o, Action onSuccessfulCopy)
		{
			BlobHttpDownloader blobHttpDownloader = ((object[])o)[0] as BlobHttpDownloader;
			BlobDbUploader blobDbUploader = ((object[])o)[1] as BlobDbUploader;
			while (true)
			{
				if (blobHttpDownloader.HasLastOperationFailed())
				{
					blobDbUploader.SetCopyFailed(new Exception("GetBlockList failed."), false, blobHttpDownloader.ErrorDescription);
					return;
				}
				if (!blobHttpDownloader.IsBlockBlob)
				{
					byte[] numArray = null;
					long num = (long)0;
					if (!blobHttpDownloader.GetNextPage(out numArray, out num))
					{
						if (blobHttpDownloader.HasLastOperationFailed())
						{
							blobDbUploader.SetCopyFailed(new Exception("Failed to download page from source."), false, blobHttpDownloader.ErrorDescription);
							return;
						}
						blobDbUploader.SetCopyCompleted();
						if (onSuccessfulCopy == null)
						{
							break;
						}
						onSuccessfulCopy();
						return;
					}
					else if (!blobDbUploader.PutPage(num, numArray))
					{
						return;
					}
				}
				else
				{
					byte[] numArray1 = null;
					string str = null;
					if (!blobHttpDownloader.GetNextBlock(out str, out numArray1))
					{
						if (!blobHttpDownloader.HasLastOperationFailed())
						{
							break;
						}
						blobDbUploader.SetCopyFailed(new Exception("Failed to download block from source."), false, blobHttpDownloader.ErrorDescription);
						return;
					}
					else if (!blobDbUploader.PutBlock(str, numArray1))
					{
						return;
					}
				}
				Thread.Sleep(1000);
			}
		}
	}
}