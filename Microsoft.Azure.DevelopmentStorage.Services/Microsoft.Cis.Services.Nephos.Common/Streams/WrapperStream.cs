using AsyncHelper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class WrapperStream : Stream
	{
		private Stream innerStream;

		private bool ownStream;

		private WrapperStream.BeforeReadHandler onBeforeRead;

		private WrapperStream.AfterReadHandler onAfterRead;

		private WrapperStream.BeforeWriteHandler onBeforeWrite;

		private WrapperStream.AfterWriteHandler onAfterWrite;

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

		public override long Length
		{
			get
			{
				this.CheckDisposed();
				return this.innerStream.Length;
			}
		}

		public WrapperStream.AfterReadHandler OnAfterRead
		{
			get
			{
				return this.onAfterRead;
			}
			set
			{
				this.onAfterRead = value;
			}
		}

		public WrapperStream.AfterWriteHandler OnAfterWrite
		{
			get
			{
				return this.onAfterWrite;
			}
			set
			{
				this.onAfterWrite = value;
			}
		}

		public WrapperStream.BeforeDisposeHandler OnBeforeDispose
		{
			get;
			set;
		}

		public WrapperStream.BeforeReadHandler OnBeforeRead
		{
			get
			{
				return this.onBeforeRead;
			}
			set
			{
				this.onBeforeRead = value;
			}
		}

		public WrapperStream.BeforeWriteHandler OnBeforeWrite
		{
			get
			{
				return this.onBeforeWrite;
			}
			set
			{
				this.onBeforeWrite = value;
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
				if (this.innerStream.CanTimeout)
				{
					this.innerStream.WriteTimeout = value;
				}
			}
		}

		public WrapperStream(Stream innerStream, bool ownStream)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("innerStream");
			}
			this.innerStream = innerStream;
			this.ownStream = ownStream;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<int> asyncIteratorContext = new AsyncIteratorContext<int>("WrapperStream.Read", callback, state);
			asyncIteratorContext.Begin(this.ReadImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("WrapperStream.Write", callback, state);
			asyncIteratorContext.Begin(this.WriteImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private void CheckDisposed()
		{
			if (this.innerStream == null)
			{
				throw new ObjectDisposedException("WrapperStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (this.OnBeforeDispose != null && !this.OnBeforeDispose())
			{
				return;
			}
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
			this.innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			this.CheckDisposed();
			if (this.OnBeforeRead != null)
			{
				this.OnBeforeRead();
			}
			int num = this.innerStream.Read(buffer, offset, count);
			if (this.OnAfterRead != null)
			{
				this.OnAfterRead(buffer, offset, num);
			}
			return num;
		}

		private IEnumerator<IAsyncResult> ReadImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<int> context)
		{
			this.CheckDisposed();
			if (this.OnBeforeRead != null)
			{
				this.OnBeforeRead();
			}
			IAsyncResult asyncResult = this.innerStream.BeginRead(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("WrapperStream.ReadImpl"));
			yield return asyncResult;
			int num = this.innerStream.EndRead(asyncResult);
			if (this.OnAfterRead != null)
			{
				this.OnAfterRead(buffer, offset, num);
			}
			context.ResultData = num;
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

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.CheckDisposed();
			if (this.OnBeforeWrite != null)
			{
				this.OnBeforeWrite(ref buffer, ref offset, ref count);
			}
			this.innerStream.Write(buffer, offset, count);
			if (this.OnAfterWrite != null)
			{
				this.OnAfterWrite(buffer, offset, count);
			}
		}

		private IEnumerator<IAsyncResult> WriteImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<NoResults> context)
		{
			this.CheckDisposed();
			if (this.OnBeforeWrite != null)
			{
				this.OnBeforeWrite(ref buffer, ref offset, ref count);
			}
			IAsyncResult asyncResult = this.innerStream.BeginWrite(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("WrapperStream.WriteImpl"));
			yield return asyncResult;
			this.innerStream.EndWrite(asyncResult);
			if (this.OnAfterWrite != null)
			{
				this.OnAfterWrite(buffer, offset, count);
			}
		}

		public delegate void AfterReadHandler(byte[] buffer, int offset, int bytesRead);

		public delegate void AfterWriteHandler(byte[] buffer, int offset, int count);

		public delegate bool BeforeDisposeHandler();

		public delegate void BeforeReadHandler();

		[SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId="0#", Justification="According to FxCop, it is safe to exclude a warning from this rule; however, this design might cause usability issues.  Since this stream is only used by internal components and not exposed to the end user of the client library, that trade-off is acceptable.")]
		public delegate void BeforeWriteHandler(ref byte[] buffer, ref int offset, ref int count);
	}
}