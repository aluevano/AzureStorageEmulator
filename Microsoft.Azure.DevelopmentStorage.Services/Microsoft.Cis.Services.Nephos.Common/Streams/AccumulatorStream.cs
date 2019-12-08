using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	[SuppressMessage("Anvil.RdUsage!Collection", "27120", Justification="There is a bug in the tool.")]
	public class AccumulatorStream : Stream
	{
		private readonly List<BufferWrapper> buffers = new List<BufferWrapper>();

		private Stream innerStream;

		private readonly int minBufferSize;

		private int bytesAccumulated;

		private int totalBufferSize;

		private readonly bool ownStream;

		public int BufferCount
		{
			get
			{
				this.CheckDisposed();
				return this.buffers.Count;
			}
		}

		public int BytesAccumulated
		{
			get
			{
				this.CheckDisposed();
				return this.bytesAccumulated;
			}
		}

		public override bool CanRead
		{
			get
			{
				this.CheckDisposed();
				return false;
			}
		}

		public override bool CanSeek
		{
			get
			{
				this.CheckDisposed();
				return false;
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
				return this.innerStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.Position;
			}
			set
			{
				throw new NotSupportedException("Seeking is not supported");
			}
		}

		public override int ReadTimeout
		{
			get
			{
				throw new NotSupportedException("reading is not supported");
			}
			set
			{
				throw new NotSupportedException("reading is not supported");
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

		public AccumulatorStream(Stream innerStream, int minBufferSize, bool ownStream)
		{
			this.innerStream = innerStream;
			this.minBufferSize = minBufferSize;
			this.ownStream = ownStream;
		}

		public IAsyncResult BeginFlushToInnerStream(TimeSpan timeout, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("AccumulatorStream.FlushToInnerStream", callback, state);
			asyncIteratorContext.Begin(this.FlushToInnerStreamImpl(timeout, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("reading is not supported");
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("AccumulatorStream.Write", callback, state);
			asyncIteratorContext.Begin(this.WriteImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private void CheckDisposed()
		{
			if (this.innerStream == null)
			{
				throw new ObjectDisposedException("AccumulatorStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && this.innerStream != null)
			{
				if (this.ownStream)
				{
					this.innerStream.Close();
				}
				this.innerStream = null;
			}
			if (!Environment.HasShutdownStarted)
			{
				foreach (BufferWrapper buffer in this.buffers)
				{
					BufferPool.ReleaseBuffer(buffer);
				}
				this.buffers.Clear();
			}
			base.Dispose(disposing);
		}

		public void EmptyStream()
		{
			this.bytesAccumulated = 0;
			this.ResetBuffers();
		}

		public void EndFlushToInnerStream(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override int EndRead(IAsyncResult asyncResult)
		{
			throw new NotImplementedException("reading is not supported");
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			Exception exception;
			if (asyncResult == null)
			{
				throw new ArgumentNullException("asyncResult");
			}
			((AsyncIteratorContext<NoResults>)asyncResult).End(out exception);
			if (exception != null)
			{
				throw exception;
			}
		}

		public override void Flush()
		{
		}

		public void FlushToInnerStream(TimeSpan timeout)
		{
			this.EndFlushToInnerStream(this.BeginFlushToInnerStream(timeout, null, null));
		}

		private IEnumerator<IAsyncResult> FlushToInnerStreamImpl(TimeSpan timeout, AsyncIteratorContext<NoResults> context)
		{
			this.CheckDisposed();
			Duration startingNow = Duration.StartingNow;
			bool flag = true;
			List<BufferWrapper>.Enumerator enumerator = this.buffers.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					byte[] numArray = enumerator.Current.Buffer;
					if (this.bytesAccumulated == 0)
					{
						goto Label0;
					}
					int num = Math.Min((int)numArray.Length, this.bytesAccumulated);
					int timeoutInMS = AsyncStreamCopy.TimeSpanToTimeoutInMS(startingNow.Remaining(timeout));
					if (timeoutInMS == 0)
					{
						throw new TimeoutException("timed out while flushing");
					}
					if (flag)
					{
						try
						{
							this.innerStream.WriteTimeout = timeoutInMS;
						}
						catch (InvalidOperationException invalidOperationException)
						{
							flag = false;
						}
					}
					IAsyncResult asyncResult = this.innerStream.BeginWrite(numArray, 0, num, context.GetResumeCallback(), context.GetResumeState("this.innerStream.Write"));
					yield return asyncResult;
					this.innerStream.EndWrite(asyncResult);
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					verbose.Log("Finished flushing one block of data into underlying stream. Data bytes: {0}", new object[] { num });
					this.bytesAccumulated -= num;
				}
				goto Label0;
				throw new TimeoutException("timed out while flushing");
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
		Label0:
			this.ResetBuffers();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("reading is not supported");
		}

		private void ResetBuffers()
		{
			if (this.buffers.Count > 1)
			{
				List<BufferWrapper> bufferWrappers = new List<BufferWrapper>(this.buffers);
				this.buffers.RemoveRange(1, this.buffers.Count - 1);
				for (int i = 1; i < bufferWrappers.Count; i++)
				{
					this.totalBufferSize -= (int)bufferWrappers[i].Buffer.Length;
				}
				for (int j = 1; j < bufferWrappers.Count; j++)
				{
					BufferPool.ReleaseBuffer(bufferWrappers[j]);
				}
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek is not supported");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength is not supported");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.EndWrite(this.BeginWrite(buffer, offset, count, null, null));
		}

		private IEnumerator<IAsyncResult> WriteImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<NoResults> context)
		{
			this.CheckDisposed();
			if (!this.innerStream.CanWrite)
			{
				throw new NotSupportedException("writing is not supported by the inner stream");
			}
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
				if (this.bytesAccumulated == this.totalBufferSize)
				{
					BufferWrapper bufferWrapper = BufferPool.GetBuffer(this.minBufferSize);
					this.buffers.Add(bufferWrapper);
					this.totalBufferSize += (int)bufferWrapper.Buffer.Length;
					if (this.totalBufferSize < 0)
					{
						throw new IOException("too many bytes have accumulated in memory");
					}
				}
				byte[] numArray = this.buffers[this.buffers.Count - 1].Buffer;
				int num = this.totalBufferSize - this.bytesAccumulated;
				int length = (int)numArray.Length - num;
				int num1 = Math.Min(num, count);
				Buffer.BlockCopy(buffer, offset, numArray, length, num1);
				count -= num1;
				offset += num1;
				this.bytesAccumulated += num1;
			}
			yield break;
		}
	}
}