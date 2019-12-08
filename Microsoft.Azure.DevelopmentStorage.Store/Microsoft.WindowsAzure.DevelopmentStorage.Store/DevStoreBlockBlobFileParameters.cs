using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class DevStoreBlockBlobFileParameters
	{
		internal long LastAllottedChunkEnd;

		internal ReaderWriterLockSlim AccessReadWriteLock
		{
			get;
			private set;
		}

		internal string DirectoryName
		{
			get;
			set;
		}

		internal string ReadOnlyFile
		{
			get;
			set;
		}

		internal string ReadWriteFile
		{
			get;
			set;
		}

		internal DevStoreBlockBlobFileParameters()
		{
			this.AccessReadWriteLock = new ReaderWriterLockSlim();
			this.ReadOnlyFile = null;
		}
	}
}