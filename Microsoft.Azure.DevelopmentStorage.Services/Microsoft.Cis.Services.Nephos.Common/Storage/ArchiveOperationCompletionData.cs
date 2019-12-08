using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class ArchiveOperationCompletionData
	{
		public string ArchiveMetadata
		{
			get;
			set;
		}

		public byte[] ArchiveMetadataBytes
		{
			get;
			set;
		}

		public string EncryptionKey
		{
			get;
			set;
		}

		public byte[] EncryptionKeyBytes
		{
			get;
			set;
		}

		public string FailureDescription
		{
			get;
			set;
		}

		public string FailureReason
		{
			get;
			set;
		}

		public int Version
		{
			get;
			set;
		}

		public ArchiveOperationCompletionData()
		{
		}
	}
}