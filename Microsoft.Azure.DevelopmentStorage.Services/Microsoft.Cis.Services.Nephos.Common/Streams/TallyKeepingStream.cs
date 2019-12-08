using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class TallyKeepingStream : Stream
	{
		private Stream innerStream;

		private long bytesWritten;

		private long bytesRead;

		private bool disposed;

		private readonly bool ownStream;

		private readonly IPerfTimer readTimer;

		private readonly IPerfTimer writeTimer;

		public long BytesRead
		{
			get
			{
				this.CheckDisposed();
				return this.bytesRead;
			}
		}

		public long BytesWritten
		{
			get
			{
				this.CheckDisposed();
				return this.bytesWritten;
			}
		}

		public override bool CanRead
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.CanSeek;
			}
		}

		public override bool CanTimeout
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.CanTimeout;
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

		public ByteCountEventArgs EventArgs
		{
			get;
			private set;
		}

		public bool IsErrorResponse
		{
			get;
			set;
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
				this.CheckDisposed();
				this.innerStream.Position = value;
			}
		}

		public TimeSpan ReadNetworkLatency
		{
			get
			{
				return this.readTimer.Elapsed;
			}
		}

		public override int ReadTimeout
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.ReadTimeout;
			}
			set
			{
				this.CheckDisposed();
				this.innerStream.ReadTimeout = value;
			}
		}

		public TimeSpan WriteNetworkLatency
		{
			get
			{
				return this.writeTimer.Elapsed;
			}
		}

		public override int WriteTimeout
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.WriteTimeout;
			}
			set
			{
				this.CheckDisposed();
				this.innerStream.WriteTimeout = value;
			}
		}

		public TallyKeepingStream(Stream innerStream, bool ownStream)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("innerStream");
			}
			this.innerStream = innerStream;
			this.ownStream = ownStream;
			bool flag = false;
			this.readTimer = PerfTimerFactory.CreateTimer(flag, new TimeSpan((long)0));
			this.writeTimer = PerfTimerFactory.CreateTimer(flag, new TimeSpan((long)0));
		}

		public TallyKeepingStream(Stream innerStream, bool ownStream, ByteCountEventArgs eventArgs) : this(innerStream, ownStream)
		{
			this.EventArgs = eventArgs;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<int> asyncIteratorContext = new AsyncIteratorContext<int>("TallyKeepingStream.Read", callback, state);
			asyncIteratorContext.Begin(this.ReadImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("TallyKeepingStream.Write", callback, state);
			asyncIteratorContext.Begin(this.WriteImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private void CheckDisposed()
		{
			if (this.disposed)
			{
				throw new ObjectDisposedException("TallyKeepingStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (this.disposeEvent != null)
				{
					try
					{
						this.disposeEvent(this, null);
					}
					finally
					{
						this.disposeEvent = null;
					}
				}
				this.writeTimer.Stop();
				this.readTimer.Stop();
				this.readingEvent = null;
				this.writingEvent = null;
				if (!this.disposed)
				{
					try
					{
						if (disposing && this.ownStream && this.innerStream != null)
						{
							this.innerStream.Dispose();
							this.innerStream = null;
						}
					}
					finally
					{
						this.disposed = true;
					}
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
			AsyncIteratorContext<int> asyncIteratorContext = (AsyncIteratorContext<int>)asyncResult;
			asyncIteratorContext.End(out exception);
			if (exception != null)
			{
				throw exception;
			}
			return asyncIteratorContext.ResultData;
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
			this.CheckDisposed();
			this.writeTimer.Start();
			this.innerStream.Flush();
			this.writeTimer.Stop();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			this.CheckDisposed();
			this.readTimer.Start();
			int num = this.innerStream.Read(buffer, offset, count);
			this.readTimer.Stop();
			if (this.readingEvent != null)
			{
				this.RecordPreOperationDataCount((long)num);
				this.readingEvent(this, this.EventArgs);
			}
			this.UpdateBytesRead((long)num);
			return num;
		}

		public override int ReadByte()
		{
			this.CheckDisposed();
			this.readTimer.Start();
			if (this.readingEvent != null)
			{
				this.RecordPreOperationDataCount((long)1);
				this.readingEvent(this, this.EventArgs);
			}
			int num = this.innerStream.ReadByte();
			this.readTimer.Stop();
			if (num != -1)
			{
				this.UpdateBytesRead((long)1);
			}
			return num;
		}

		private IEnumerator<IAsyncResult> ReadImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<int> context)
		{
			IAsyncResult asyncResult = null;
			this.CheckDisposed();
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("The number of bytes to read cannot be negative", "count");
			}
			this.readTimer.Start();
			if (count > 0)
			{
				asyncResult = this.innerStream.BeginRead(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("TallyKeepingStream.ReadImpl"));
				yield return asyncResult;
			}
			int num = 0;
			if (count > 0)
			{
				NephosAssertionException.Assert(asyncResult != null);
				num = this.innerStream.EndRead(asyncResult);
			}
			this.readTimer.Stop();
			if (num > 0 && this.readingEvent != null)
			{
				this.RecordPreOperationDataCount((long)num);
				this.readingEvent(this, this.EventArgs);
			}
			this.UpdateBytesRead((long)num);
			context.ResultData = num;
		}

		private void RecordPreOperationDataCount(long dataBytes)
		{
			if (this.EventArgs != null)
			{
				this.EventArgs.Bytes = dataBytes;
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			this.CheckDisposed();
			return this.innerStream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			this.CheckDisposed();
			this.innerStream.SetLength(value);
		}

		private void UpdateBytesRead(long bytesRead)
		{
			this.bytesRead += bytesRead;
			if (this.bytesReadEvent != null)
			{
				this.bytesReadEvent(this, new ByteCountEventArgs(bytesRead));
			}
		}

		private void UpdateBytesWritten(long bytesWritten)
		{
			this.bytesWritten += bytesWritten;
			if (this.bytesWrittenEvent != null)
			{
				this.bytesWrittenEvent(this, new ByteCountEventArgs(bytesWritten));
			}
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CheckDisposed();
			this.writeTimer.Start();
			if (this.writingEvent != null)
			{
				this.RecordPreOperationDataCount((long)count);
				this.writingEvent(this, this.EventArgs);
			}
			this.innerStream.Write(buffer, offset, count);
			this.writeTimer.Stop();
			this.UpdateBytesWritten((long)count);
		}

		public override void WriteByte(byte value)
		{
			this.CheckDisposed();
			this.writeTimer.Start();
			if (this.writingEvent != null)
			{
				this.RecordPreOperationDataCount((long)1);
				this.writingEvent(this, this.EventArgs);
			}
			this.innerStream.WriteByte(value);
			this.writeTimer.Stop();
			this.UpdateBytesWritten((long)1);
		}

		private IEnumerator<IAsyncResult> WriteImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<NoResults> context)
		{
			this.CheckDisposed();
			this.writeTimer.Start();
			if (this.writingEvent != null)
			{
				this.RecordPreOperationDataCount((long)count);
				this.writingEvent(this, this.EventArgs);
			}
			IAsyncResult asyncResult = this.innerStream.BeginWrite(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("TallyKeepingStream.WriteImpl"));
			yield return asyncResult;
			this.innerStream.EndWrite(asyncResult);
			this.writeTimer.Stop();
			this.UpdateBytesWritten((long)count);
		}

		private event EventHandler<ByteCountEventArgs> bytesReadEvent;

		public event EventHandler<ByteCountEventArgs> BytesReadEvent
		{
			add
			{
				this.bytesReadEvent += value;
			}
			remove
			{
				this.bytesReadEvent -= value;
			}
		}

		private event EventHandler<ByteCountEventArgs> bytesWrittenEvent;

		public event EventHandler<ByteCountEventArgs> BytesWrittenEvent
		{
			add
			{
				this.bytesWrittenEvent += value;
			}
			remove
			{
				this.bytesWrittenEvent -= value;
			}
		}

		private event EventHandler disposeEvent;

		public event EventHandler DisposingEvent
		{
			add
			{
				this.disposeEvent += value;
			}
			remove
			{
				this.disposeEvent -= value;
			}
		}

		private event EventHandler<ByteCountEventArgs> readingEvent;

		public event EventHandler<ByteCountEventArgs> ReadingEvent
		{
			add
			{
				this.readingEvent += value;
			}
			remove
			{
				this.readingEvent -= value;
			}
		}

		private event EventHandler<ByteCountEventArgs> writingEvent;

		public event EventHandler<ByteCountEventArgs> WritingEvent
		{
			add
			{
				this.writingEvent += value;
			}
			remove
			{
				this.writingEvent -= value;
			}
		}
	}
}