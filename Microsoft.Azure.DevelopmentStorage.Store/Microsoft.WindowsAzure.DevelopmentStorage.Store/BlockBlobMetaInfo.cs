using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class BlockBlobMetaInfo
	{
		internal string AccountName
		{
			get;
			private set;
		}

		internal string BlobDirectory
		{
			get;
			private set;
		}

		internal string BlobName
		{
			get;
			private set;
		}

		internal long BlobSize
		{
			get;
			set;
		}

		internal string ContainerName
		{
			get;
			private set;
		}

		internal BlockBlobMetaInfo(string accountName, string containerName, string blobName, string directoryPath)
		{
			this.AccountName = accountName;
			this.ContainerName = containerName;
			this.BlobName = blobName;
			this.BlobDirectory = directoryPath;
		}

		public string GetKey()
		{
			object[] accountName = new object[] { this.AccountName, this.ContainerName, this.BlobName, this.BlobDirectory };
			return string.Format("{0}_{1}_{2}_{3}", accountName).ToLowerInvariant();
		}

		public override string ToString()
		{
			object[] blobDirectory = new object[] { this.BlobDirectory, this.BlobName, this.ContainerName, this.BlobSize };
			return string.Format("BlobDirectory = {0}, BlobName = {1}, ContainerName = {2}, BlobSize = {3}", blobDirectory);
		}
	}
}