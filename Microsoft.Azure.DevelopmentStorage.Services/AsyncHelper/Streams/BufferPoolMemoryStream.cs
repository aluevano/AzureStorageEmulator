using AsyncHelper;
using System;
using System.Collections.Generic;
using System.IO;

namespace AsyncHelper.Streams
{
	public class BufferPoolMemoryStream : Stream
	{
		private readonly List<BufferWrapper> buffers = new List<BufferWrapper>();

		private int currentBufferIndex;

		private int currentBufferOffset;

		private readonly int minBufferSize;

		private long streamLength;

		private long streamPosition;

		private int totalBufferSize;

		private readonly IBufferManager bufferManager;

		private bool disposed;

		internal List<BufferWrapper> Buffers
		{
			get
			{
				this.CheckDisposed();
				return this.buffers;
			}
		}

		public override bool CanRead
		{
			get
			{
				this.CheckDisposed();
				return true;
			}
		}

		public override bool CanSeek
		{
			get
			{
				this.CheckDisposed();
				return true;
			}
		}

		public override bool CanTimeout
		{
			get
			{
				this.CheckDisposed();
				return false;
			}
		}

		public override bool CanWrite
		{
			get
			{
				this.CheckDisposed();
				return true;
			}
		}

		public override long Length
		{
			get
			{
				this.CheckDisposed();
				return this.streamLength;
			}
		}

		public override long Position
		{
			get
			{
				this.CheckDisposed();
				return this.streamPosition;
			}
			set
			{
				if (value < (long)0 || value > this.streamLength)
				{
					throw new ArgumentOutOfRangeException("value", "Cannot seek outside the length of the stream");
				}
				this.streamPosition = value;
				long length = this.streamPosition;
				this.currentBufferOffset = 0;
				this.currentBufferIndex = 0;
				while (this.currentBufferIndex < this.buffers.Count)
				{
					if (length < (long)((int)this.buffers[this.currentBufferIndex].Buffer.Length))
					{
						this.currentBufferOffset = (int)length;
						return;
					}
					length -= (long)((int)this.buffers[this.currentBufferIndex].Buffer.Length);
					this.currentBufferIndex++;
				}
			}
		}

		public override int ReadTimeout
		{
			get
			{
				throw new NotSupportedException("ReadTimeout is not supported");
			}
			set
			{
				throw new NotSupportedException("ReadTimeout is not supported");
			}
		}

		public override int WriteTimeout
		{
			get
			{
				throw new NotSupportedException("WriteTimeout is not supported");
			}
			set
			{
				throw new NotSupportedException("WriteTimeout is not supported");
			}
		}

		public BufferPoolMemoryStream() : this(65536)
		{
		}

		public BufferPoolMemoryStream(int minBufferSize) : this(minBufferSize, DefaultBufferManager.Instance)
		{
		}

		internal BufferPoolMemoryStream(int minBufferSize, IBufferManager bufferManager)
		{
			this.minBufferSize = minBufferSize;
			this.bufferManager = bufferManager;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncResult<int> asyncResult = new AsyncResult<int>("BufferPoolMemoryStream.Read", callback, state);
			try
			{
				asyncResult.ResultData = this.Read(buffer, offset, count);
				asyncResult.Complete(null, true);
			}
			catch (Exception exception)
			{
				asyncResult.Complete(exception, true);
			}
			return asyncResult;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncResult<NoResults> asyncResult = new AsyncResult<NoResults>("BufferPoolMemoryStream.Write", callback, state);
			try
			{
				this.Write(buffer, offset, count);
				asyncResult.Complete(null, true);
			}
			catch (Exception exception)
			{
				asyncResult.Complete(exception, true);
			}
			return asyncResult;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("BufferPoolMemoryStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (!this.disposed && !Environment.HasShutdownStarted)
				{
					this.disposed = true;
					foreach (BufferWrapper buffer in this.buffers)
					{
						this.bufferManager.ReleaseBuffer(buffer);
					}
					this.buffers.Clear();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			AsyncResult<int> asyncResult1 = (AsyncResult<int>)asyncResult;
			asyncResult1.End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
			return asyncResult1.ResultData;
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			((AsyncResult<NoResults>)asyncResult).End(out exception, RethrowableWrapperBehavior.Wrap);
			if (exception != null)
			{
				throw exception;
			}
		}

		protected override void Finalize()
		{
			try
			{
				this.Dispose(false);
			}
			finally
			{
				base.Finalize();
			}
		}

		public override void Flush()
		{
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "cannot be negative");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "cannot be negative");
			}
			if ((int)buffer.Length - offset < count)
			{
				throw new ArgumentException("count is greater than the number of available bytes starting at this offset");
			}
			int num = 0;
			while (count > 0 && this.streamLength != this.streamPosition)
			{
				byte[] numArray = this.buffers[this.currentBufferIndex].Buffer;
				int num1 = (int)Math.Min(this.streamLength - this.streamPosition, (long)((int)numArray.Length - this.currentBufferOffset));
				int num2 = Math.Min(num1, count);
				Buffer.BlockCopy(numArray, this.currentBufferOffset, buffer, offset, num2);
				num += num2;
				this.currentBufferOffset += num2;
				count -= num2;
				offset += num2;
				this.streamPosition += (long)num2;
				if (this.currentBufferOffset != (int)this.buffers[this.currentBufferIndex].Buffer.Length)
				{
					continue;
				}
				this.currentBufferIndex++;
				this.currentBufferOffset = 0;
			}
			return num;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			long length = (long)0;
			switch (origin)
			{
				case SeekOrigin.Begin:
				{
					length = (long)0;
					break;
				}
				case SeekOrigin.Current:
				{
					length = this.streamPosition;
					break;
				}
				case SeekOrigin.End:
				{
					length = this.Length;
					break;
				}
				default:
				{
					throw new ArgumentOutOfRangeException("origin");
				}
			}
			long num = length + offset;
			if (num < (long)0 || num > this.Length)
			{
				throw new InvalidOperationException("Cannot seek outside the bounds of the stream");
			}
			this.Position = num;
			return this.Position;
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength is not supported");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CheckDisposed();
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", "cannot be negative");
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", "cannot be negative");
			}
			if ((int)buffer.Length - offset < count)
			{
				throw new ArgumentException("count is greater than the number of available bytes starting at this offset");
			}
			if (offset >= (int)buffer.Length)
			{
				throw new ArgumentException("offset is too large for the given buffer");
			}
			while (count > 0)
			{
				if (this.streamPosition != (long)this.totalBufferSize)
				{
					byte[] numArray = this.buffers[this.currentBufferIndex].Buffer;
					int length = (int)numArray.Length - this.currentBufferOffset;
					int num = Math.Min(length, count);
					Buffer.BlockCopy(buffer, offset, numArray, this.currentBufferOffset, num);
					this.currentBufferOffset += num;
					this.streamPosition += (long)num;
					this.streamLength = Math.Max(this.streamLength, this.streamPosition);
					count -= num;
					offset += num;
					if (this.currentBufferOffset != (int)this.buffers[this.currentBufferIndex].Buffer.Length)
					{
						continue;
					}
					this.currentBufferIndex++;
					this.currentBufferOffset = 0;
				}
				else
				{
					BufferWrapper bufferWrapper = this.bufferManager.GetBuffer(this.minBufferSize);
					this.buffers.Add(bufferWrapper);
					this.totalBufferSize += (int)bufferWrapper.Buffer.Length;
					if (this.totalBufferSize >= 0)
					{
						continue;
					}
					throw new IOException("too many bytes have accumulated in memory");
				}
			}
		}
	}
}