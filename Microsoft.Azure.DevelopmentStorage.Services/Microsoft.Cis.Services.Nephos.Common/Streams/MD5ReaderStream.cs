using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class MD5ReaderStream : Stream
	{
		private MD5 md5;

		private Stream innerStream;

		private bool ownStream;

		private long streamLength;

		private byte[] hashToVerifyAgainst;

		private byte[] computedHash;

		private long streamPosition;

		private readonly IPerfTimer readTimer;

		private RequestContext requestContext;

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
				return false;
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
				return false;
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		public byte[] Hash
		{
			get
			{
				return this.computedHash;
			}
		}

		public string HashAsBase64
		{
			get
			{
				return Convert.ToBase64String(this.Hash);
			}
		}

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		public byte[] HashToVerifyAgainst
		{
			get
			{
				return this.hashToVerifyAgainst;
			}
			set
			{
				if (value != null)
				{
					int hashSize = this.md5.HashSize / 8;
					if (hashSize != (int)value.Length)
					{
						CultureInfo invariantCulture = CultureInfo.InvariantCulture;
						object[] length = new object[] { hashSize, (int)value.Length };
						throw new MD5InvalidException(string.Format(invariantCulture, "Expected hash length of {0} bytes but actual length was {1} bytes", length));
					}
				}
				this.hashToVerifyAgainst = value;
			}
		}

		public string HashToVerifyAgainstAsBase64
		{
			get
			{
				return Convert.ToBase64String(this.HashToVerifyAgainst);
			}
			set
			{
				if (value == null)
				{
					this.HashToVerifyAgainst = null;
					return;
				}
				this.HashToVerifyAgainst = Convert.FromBase64String(value);
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
				return this.innerStream.Position;
			}
			set
			{
				throw new NotSupportedException("MD5ReaderStream does not support seeking");
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
				if (this.innerStream.CanTimeout)
				{
					this.innerStream.ReadTimeout = value;
				}
			}
		}

		public override int WriteTimeout
		{
			get
			{
				throw new NotSupportedException("MD5ReaderStream does not support writing");
			}
			set
			{
				throw new NotSupportedException("MD5ReaderStream does not support writing");
			}
		}

		public MD5ReaderStream(Stream innerStream, long streamLength, RequestContext requestContext) : this(innerStream, streamLength, true, requestContext)
		{
		}

		public MD5ReaderStream(Stream innerStream, long streamLength, bool ownStream, RequestContext requestContext)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("innerStream");
			}
			this.md5 = MD5.Create();
			this.innerStream = innerStream;
			this.ownStream = ownStream;
			this.streamLength = streamLength;
			bool flag = false;
			this.readTimer = PerfTimerFactory.CreateTimer(flag, new TimeSpan((long)0));
			this.requestContext = requestContext;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<int> asyncIteratorContext = new AsyncIteratorContext<int>("MD5ReaderStream.Read", callback, state);
			asyncIteratorContext.Begin(this.ReadImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("BeginWrite is not supported by MD5ReaderStream");
		}

		private void CheckDisposed()
		{
			if (this.innerStream == null)
			{
				throw new ObjectDisposedException("MD5ReaderStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				this.readTimer.Stop();
				this.readingEvent = null;
				if (disposing)
				{
					try
					{
						if (this.innerStream != null && this.ownStream)
						{
							this.innerStream.Dispose();
						}
					}
					finally
					{
						this.innerStream = null;
						try
						{
							if (this.md5 != null)
							{
								this.md5.Dispose();
							}
						}
						finally
						{
							this.md5 = null;
						}
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
			throw new NotImplementedException("EndWrite is not supported by MD5ReaderStream");
		}

		public override void Flush()
		{
			throw new NotSupportedException("Flush is not supported");
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			this.readTimer.Start();
			if (this.readingEvent != null)
			{
				this.readingEvent(this, null);
			}
			int num = this.EndRead(this.BeginRead(buffer, offset, count, null, null));
			this.readTimer.Stop();
			return num;
		}

		private IEnumerator<IAsyncResult> ReadImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<int> context)
		{
			string base64String;
			this.CheckDisposed();
			this.readTimer.Start();
			if (this.readingEvent != null)
			{
				this.readingEvent(this, null);
			}
			IAsyncResult asyncResult = this.innerStream.BeginRead(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("MD5ReaderStream.ReadImpl"));
			yield return asyncResult;
			int num = this.innerStream.EndRead(asyncResult);
			this.readTimer.Stop();
			this.streamPosition += (long)num;
			if (this.streamPosition >= this.streamLength)
			{
				if (this.streamPosition != this.streamLength)
				{
					throw new InvalidStreamLengthException(string.Format("Expected stream length of {0} but current offset is {1}", this.streamLength, this.streamPosition));
				}
				if (this.computedHash == null)
				{
					this.md5.TransformBlock(buffer, offset, num, null, 0);
					this.md5.TransformFinalBlock(buffer, 0, 0);
					this.computedHash = this.md5.Hash;
					string str = Convert.ToBase64String(this.computedHash);
					if (this.hashToVerifyAgainst == null)
					{
						base64String = null;
					}
					else
					{
						base64String = Convert.ToBase64String(this.hashToVerifyAgainst);
					}
					string str1 = base64String;
					if (this.requestContext != null)
					{
						this.requestContext.RequestContentMD5 = str1;
						this.requestContext.ServerContentMD5 = str;
					}
					IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
					object[] objArray = new object[] { str, null, null };
					object[] objArray1 = objArray;
					string str2 = str1;
					if (str2 == null)
					{
						str2 = "NULL";
					}
					objArray1[1] = str2;
					objArray[2] = this.streamLength;
					verbose.Log("CalculatedMD5 = {0} ReceivedMD5 = {1} DataLength = {2}", objArray);
					if (this.hashToVerifyAgainst != null)
					{
						for (int i = 0; i < (int)this.hashToVerifyAgainst.Length; i++)
						{
							if (this.hashToVerifyAgainst[i] != this.md5.Hash[i])
							{
								throw new MD5MismatchException("MD5ReaderStream: Computed hash does not match supplied hash", this.hashToVerifyAgainst, this.md5.Hash);
							}
						}
					}
				}
			}
			else
			{
				if (num == 0)
				{
					throw new InvalidStreamLengthException(string.Format("Expected stream length of {0} but actual is {1}", this.streamLength, this.streamPosition));
				}
				this.md5.TransformBlock(buffer, offset, num, null, 0);
			}
			context.ResultData = num;
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek is not supported by MD5ReaderStream");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength is not supported by MD5ReaderStream");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write is not supported by MD5ReaderStream");
		}

		private event EventHandler readingEvent;

		public event EventHandler ReadingEvent
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
	}
}