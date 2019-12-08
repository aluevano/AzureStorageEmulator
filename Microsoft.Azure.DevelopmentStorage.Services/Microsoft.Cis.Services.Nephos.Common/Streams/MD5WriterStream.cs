using AsyncHelper;
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

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class MD5WriterStream : CrcStream
	{
		public const long MaxSupportedStreamLength = 4194304L;

		private MD5 md5;

		private CrcStream innerStream;

		private bool ownStream;

		private bool synchronousInnerStream;

		private long streamLength;

		private byte[] hashToVerifyAgainst;

		private byte[] computedHash;

		private long streamPosition;

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

		[SuppressMessage("Microsoft.Performance", "CA1819:PropertiesShouldNotReturnArrays", Justification="Seeing as this property is only used among internal components and not exposed to the end user, it is deemed safe to expose the array directly for performance and ease of use.")]
		public byte[] Hash
		{
			get
			{
				if (this.computedHash == null)
				{
					throw new InvalidOperationException("Hash cannot be retrieved until end of stream has been reached");
				}
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
				throw new NotSupportedException("MD5WriterStream does not support seeking");
			}
		}

		public override int ReadTimeout
		{
			get
			{
				throw new NotSupportedException("MD5WriterStream does not support reading");
			}
			set
			{
				throw new NotSupportedException("MD5WriterStream does not support reading");
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

		public MD5WriterStream(CrcStream innerStream, long streamLength) : this(innerStream, streamLength, true)
		{
		}

		public MD5WriterStream(CrcStream innerStream, long streamLength, bool ownStream, bool synchronousInnerStream)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("innerStream");
			}
			if (streamLength > (long)4194304)
			{
				throw new ArgumentOutOfRangeException("streamLength", string.Format("MD5WriterStream supports a maximum stream length of {0}.", (long)4194304));
			}
			this.md5 = MD5.Create();
			this.innerStream = innerStream;
			this.ownStream = ownStream;
			this.streamLength = streamLength;
			this.streamPosition = (long)0;
			this.synchronousInnerStream = synchronousInnerStream;
		}

		public MD5WriterStream(CrcStream innerStream, long streamLength, bool ownStream) : this(innerStream, streamLength, ownStream, false)
		{
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("BeginRead is not supported by MD5WriterStream");
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			long? nullable = null;
			return this.BeginWrite(buffer, offset, count, nullable, callback, state);
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, long? crc, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<NoResults> asyncIteratorContext = new AsyncIteratorContext<NoResults>("MD5WriterStream.Write", callback, state);
			asyncIteratorContext.Begin(this.WriteImpl(buffer, offset, count, crc, asyncIteratorContext));
			return asyncIteratorContext;
		}

		private void CheckDisposed()
		{
			if (this.innerStream == null)
			{
				throw new ObjectDisposedException("MD5WriterStream");
			}
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
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
			throw new NotImplementedException("EndRead is not supported by MD5WriterStream");
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
			throw new NotSupportedException("Read is not supported by MD5WriterStream");
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek is not supported by MD5WriterStream");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength is not supported by MD5WriterStream");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			this.EndWrite(this.BeginWrite(buffer, offset, count, null, null));
		}

		private IEnumerator<IAsyncResult> WriteImpl(byte[] buffer, int offset, int count, long? crc, AsyncIteratorContext<NoResults> context)
		{
			object obj;
			this.CheckDisposed();
			if (buffer == null)
			{
				throw new ArgumentNullException("buffer");
			}
			if (offset < 0)
			{
				throw new ArgumentOutOfRangeException("offset", string.Format("Offset ({0}) must be positive.", offset));
			}
			if (count < 0)
			{
				throw new ArgumentOutOfRangeException("count", string.Format("Count ({0}) must be positive.", count));
			}
			if (count + offset > (int)buffer.Length)
			{
				throw new ArgumentException(string.Format("The sum of Offset ({0}) and Count ({1}) must be less than the buffer length ({2}).", offset, count, (int)buffer.Length));
			}
			if (this.streamPosition + (long)count > this.streamLength)
			{
				object[] objArray = new object[] { this.streamLength, this.streamPosition, count, this.streamPosition + (long)count };
				throw new InvalidStreamLengthException(string.Format("Expected {0} bytes to be written, but at current position {1}, writing {2} bytes would result in length {3}. Unable to calculate the MD5 given this error.", objArray));
			}
			if (!this.synchronousInnerStream)
			{
				IAsyncResult asyncResult = this.innerStream.BeginWrite(buffer, offset, count, crc, context.GetResumeCallback(), context.GetResumeState("MD5WriterStream.WriteImpl"));
				yield return asyncResult;
				this.innerStream.EndWrite(asyncResult);
			}
			else
			{
				this.innerStream.Write(buffer, offset, count, crc);
			}
			this.streamPosition += (long)count;
			if (this.streamPosition < this.streamLength)
			{
				this.md5.TransformBlock(buffer, offset, count, null, 0);
			}
			else if (this.streamPosition == this.streamLength && this.computedHash == null)
			{
				this.md5.TransformBlock(buffer, offset, count, null, 0);
				this.md5.TransformFinalBlock(buffer, 0, 0);
				this.computedHash = this.md5.Hash;
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] base64String = new object[] { Convert.ToBase64String(this.computedHash), null, null };
				object[] objArray1 = base64String;
				obj = (this.hashToVerifyAgainst == null ? "NULL" : Convert.ToBase64String(this.hashToVerifyAgainst));
				objArray1[1] = obj;
				base64String[2] = this.streamLength;
				verbose.Log("CalculatedMD5 = {0} MD5ToVerifyAgainst = {1} DataLength = {2}", base64String);
				if (this.hashToVerifyAgainst != null)
				{
					for (int i = 0; i < (int)this.hashToVerifyAgainst.Length; i++)
					{
						if (this.hashToVerifyAgainst[i] != this.md5.Hash[i])
						{
							throw new MD5MismatchException("MD5WriterStream: Computed hash does not match supplied hash", this.hashToVerifyAgainst, this.md5.Hash);
						}
					}
				}
			}
			else if (this.streamPosition > this.streamLength)
			{
				throw new InvalidStreamLengthException(string.Format("Bytes written ({0}) exceed expected stream length {1}. Unable to calculate the MD5 due to this error.", this.streamPosition, this.streamLength));
			}
		}
	}
}