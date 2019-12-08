using System;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public abstract class CrcStream : Stream
	{
		protected CrcStream()
		{
		}

		public virtual IAsyncResult BeginWrite(byte[] buffer, int offset, int count, long? crc, AsyncCallback callback, object state)
		{
			return this.BeginWrite(buffer, offset, count, callback, state);
		}

		public virtual int EndRead(out long? crc, IAsyncResult asyncResult)
		{
			crc = null;
			return this.EndRead(asyncResult);
		}

		public virtual void Write(byte[] buffer, int offset, int count, long? crc)
		{
			this.Write(buffer, offset, count);
		}
	}
}