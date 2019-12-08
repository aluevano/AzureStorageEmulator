using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class BlobRegion : IBlobRegion, IBaseRegion
	{
		private byte[] blockIdentifier;

		private long length;

		private long offset;

		public byte[] BlockIdentifier
		{
			get
			{
				return JustDecompileGenerated_get_BlockIdentifier();
			}
			set
			{
				JustDecompileGenerated_set_BlockIdentifier(value);
			}
		}

		public byte[] JustDecompileGenerated_get_BlockIdentifier()
		{
			return this.blockIdentifier;
		}

		public void JustDecompileGenerated_set_BlockIdentifier(byte[] value)
		{
			this.blockIdentifier = value;
		}

		public long Length
		{
			get
			{
				return JustDecompileGenerated_get_Length();
			}
			set
			{
				JustDecompileGenerated_set_Length(value);
			}
		}

		public long JustDecompileGenerated_get_Length()
		{
			return this.length;
		}

		public void JustDecompileGenerated_set_Length(long value)
		{
			this.length = value;
		}

		public long Offset
		{
			get
			{
				return JustDecompileGenerated_get_Offset();
			}
			set
			{
				JustDecompileGenerated_set_Offset(value);
			}
		}

		public long JustDecompileGenerated_get_Offset()
		{
			return this.offset;
		}

		public void JustDecompileGenerated_set_Offset(long value)
		{
			this.offset = value;
		}

		public BlobRegion()
		{
		}

		public BlobRegion(long offset, long length)
		{
			this.offset = offset;
			this.length = length;
		}

		public BlobRegion(byte[] blockIdentifier, long offset, long length)
		{
			if (blockIdentifier == null)
			{
				throw new ArgumentNullException("blockIdentifier");
			}
			this.blockIdentifier = blockIdentifier;
			this.length = length;
			this.offset = offset;
		}
	}
}