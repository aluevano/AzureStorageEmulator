using System;

namespace AsyncHelper
{
	public class BufferWrapper
	{
		private byte[] buffer;

		public byte[] Buffer
		{
			get
			{
				return this.buffer;
			}
		}

		public BufferWrapper(byte[] buffer)
		{
			this.buffer = buffer;
		}

		internal virtual void ClearBuffer()
		{
			this.buffer = null;
		}
	}
}