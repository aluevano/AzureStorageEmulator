using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class CrcMemoryStream : CrcStream
	{
		private readonly MemoryStream memoryStream;

		private readonly BufferWrapper bufferWrapper;

		private bool invalidCrc;

		public override bool CanRead
		{
			get
			{
				return this.memoryStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this.memoryStream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.memoryStream.CanWrite;
			}
		}

		public virtual int Capacity
		{
			get
			{
				return this.memoryStream.Capacity;
			}
			set
			{
				this.memoryStream.Capacity = value;
			}
		}

		public long? DataCrc
		{
			get;
			private set;
		}

		public override long Length
		{
			get
			{
				return this.memoryStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.memoryStream.Position;
			}
			set
			{
				this.memoryStream.Position = value;
			}
		}

		public CrcMemoryStream(byte[] buffer) : this(buffer, 0, (int)buffer.Length, null, true)
		{
		}

		public CrcMemoryStream(byte[] buffer, int index, int count) : this(buffer, index, count, null, true)
		{
		}

		public CrcMemoryStream(byte[] buffer, int index, int count, long? crcValue, bool writable)
		{
			this.DataCrc = crcValue;
			this.memoryStream = new MemoryStream(buffer, index, count, writable, true);
		}

		public CrcMemoryStream(BufferWrapper bufferWrapper, int index, int count, long? crcValue, bool writable) : this(bufferWrapper.Buffer, index, count, crcValue, writable)
		{
			this.bufferWrapper = bufferWrapper;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncResult<AsyncHelper.Tuple<int, long?>> asyncResult = new AsyncResult<AsyncHelper.Tuple<int, long?>>("CrcMemoryStream.BeginRead", callback, state);
			Exception exception = null;
			try
			{
				int num = this.memoryStream.Read(buffer, offset, count);
				long? nullable = null;
				if (this.DataCrc.HasValue && num > 0)
				{
					nullable = ((long)num != this.Length ? new long?(this.CalculatePartialDataCrc(this.Position - (long)num, num)) : this.DataCrc);
				}
				asyncResult.ResultData = new AsyncHelper.Tuple<int, long?>(num, nullable);
			}
			catch (Exception exception1)
			{
				exception = exception1;
			}
			asyncResult.Complete(exception, true);
			return asyncResult;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			long? nullable = null;
			return this.BeginWrite(buffer, offset, count, nullable, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, long? crc, AsyncCallback callback, object state)
		{
			Exception exception = null;
			try
			{
				this.UpdateDataCrc(buffer, offset, count, crc);
				this.Write(buffer, offset, count);
			}
			catch (Exception exception1)
			{
				exception = exception1;
			}
			AsyncResult<NoResults> asyncResult = new AsyncResult<NoResults>("CrcMemoryStream.BeginWrite", callback, state);
			asyncResult.Complete(exception, true);
			return asyncResult;
		}

		private long CalculatePartialDataCrc(long offset, int count)
		{
			long num;
			long num1;
			long num2;
			byte[] buffer = this.memoryStream.GetBuffer();
			if (offset == (long)0)
			{
				int length = (int)buffer.Length;
				long? dataCrc = this.DataCrc;
				CrcUtils.SplitCalculateDataCrc(count - 1, length, buffer, dataCrc.Value, out num, out num1);
				return num;
			}
			if ((long)count + offset >= this.Length)
			{
				int length1 = (int)buffer.Length;
				long? nullable = this.DataCrc;
				CrcUtils.SplitCalculateDataCrc((int)offset - 1, length1, buffer, nullable.Value, out num, out num1);
				return num1;
			}
			int length2 = (int)buffer.Length;
			long? dataCrc1 = this.DataCrc;
			CrcUtils.SplitCalculateDataCrc((int)offset - 1, count + (int)offset - 1, length2, buffer, dataCrc1.Value, out num, out num1, out num2);
			return num1;
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing)
				{
					this.memoryStream.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			long? nullable;
			return this.EndRead(out nullable, asyncResult);
		}

		public override int EndRead(out long? crc, IAsyncResult asyncResult)
		{
			AsyncResult<AsyncHelper.Tuple<int, long?>> asyncResult1 = asyncResult as AsyncResult<AsyncHelper.Tuple<int, long?>>;
			if (asyncResult1 == null)
			{
				throw new ArgumentException("Expected asyncResult to be of type AsyncResult<AsyncHelper.Tuple<int, long?>>");
			}
			Exception exception = null;
			asyncResult1.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			crc = asyncResult1.ResultData.Second;
			return asyncResult1.ResultData.First;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			AsyncResult<NoResults> asyncResult1 = asyncResult as AsyncResult<NoResults>;
			if (asyncResult1 == null)
			{
				throw new ArgumentException("Expected asyncResult to be of type AsyncResult<NoResults>");
			}
			Exception exception = null;
			asyncResult1.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override void Flush()
		{
			this.memoryStream.Flush();
		}

		public virtual byte[] GetBuffer()
		{
			return this.memoryStream.GetBuffer();
		}

		public virtual BufferWrapper GetBufferWrapper()
		{
			return this.bufferWrapper;
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			return this.memoryStream.Read(buffer, offset, count);
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return this.memoryStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.memoryStream.SetLength(value);
		}

		public virtual byte[] ToArray()
		{
			return this.memoryStream.ToArray();
		}

		private void UpdateDataCrc(byte[] buffer, int offset, int count, long? crc)
		{
			if (this.invalidCrc || count == 0)
			{
				return;
			}
			if (!crc.HasValue || this.Position != (long)0 && !this.DataCrc.HasValue)
			{
				this.invalidCrc = true;
				this.DataCrc = null;
				return;
			}
			if (this.Position == (long)0)
			{
				this.DataCrc = crc;
				return;
			}
			long? dataCrc = this.DataCrc;
			this.DataCrc = new long?(CrcUtils.ConcatenateCrc(dataCrc.Value, this.Position, crc.Value, (long)count));
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.Write(buffer, offset, count, null);
		}

		public override void Write(byte[] buffer, int offset, int count, long? crc)
		{
			this.UpdateDataCrc(buffer, offset, count, crc);
			this.memoryStream.Write(buffer, offset, count);
		}
	}
}