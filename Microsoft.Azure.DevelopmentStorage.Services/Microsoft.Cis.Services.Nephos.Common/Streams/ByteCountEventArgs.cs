using System;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class ByteCountEventArgs : EventArgs
	{
		private long bytes;

		public long Bytes
		{
			get
			{
				return this.bytes;
			}
			set
			{
				this.bytes = value;
			}
		}

		public ByteCountEventArgs(long bytes)
		{
			this.bytes = bytes;
		}

		protected ByteCountEventArgs()
		{
			this.bytes = (long)0;
		}
	}
}