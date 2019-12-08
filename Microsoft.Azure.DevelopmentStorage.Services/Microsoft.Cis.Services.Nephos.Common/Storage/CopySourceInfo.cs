using System;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class CopySourceInfo
	{
		public byte[] ApplicationMetadata
		{
			get;
			set;
		}

		public long ContentLength
		{
			get;
			set;
		}

		public string ContentType
		{
			get;
			set;
		}

		public TimeSpan CopySourceVerificationRequestRoundTripLatency
		{
			get;
			set;
		}

		public bool IsSourceAzureBlob
		{
			get;
			set;
		}

		public Microsoft.Cis.Services.Nephos.Common.Storage.SequenceNumberUpdate SequenceNumberUpdate
		{
			get;
			set;
		}

		public NameValueCollection ServiceMetadataCollection
		{
			get;
			set;
		}

		public CopyBlobType2 SourceBlobType
		{
			get;
			set;
		}

		public string SourceETag
		{
			get;
			set;
		}

		public CopySourceInfo()
		{
			this.CopySourceVerificationRequestRoundTripLatency = TimeSpan.Zero;
		}
	}
}