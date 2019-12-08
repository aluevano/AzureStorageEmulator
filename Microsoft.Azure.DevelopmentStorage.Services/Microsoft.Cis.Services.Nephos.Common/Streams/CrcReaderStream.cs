using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class CrcReaderStream : Stream
	{
		public const long MaxSupportedStreamLength = 4194304L;

		private Stream innerStream;

		private bool ownStream;

		private long streamLength;

		private long streamPosition;

		private long currentCrc;

		private long calculatedCrc;

		private List<long> crcArray;

		private readonly int maxCrcRangeSize;

		private readonly int pageOffset;

		private int currentCrcRangeLength;

		private long currentCrcDataLength;

		private readonly IPerfTimer crcCalcuationTimer;

		private RequestContext requestContext;

		public long CalculatedCrc
		{
			get
			{
				return this.calculatedCrc;
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

		public long? Crc64ToVerifyAgainst
		{
			get;
			set;
		}

		public TimeSpan CrcCalculationDuration
		{
			get
			{
				return this.crcCalcuationTimer.Elapsed;
			}
		}

		public long[] CrcRangeArray
		{
			get
			{
				if (this.maxCrcRangeSize > 0)
				{
					return this.crcArray.ToArray();
				}
				return new long[] { this.currentCrc };
			}
		}

		public int CrcRangeSize
		{
			get
			{
				return this.maxCrcRangeSize;
			}
		}

		public long CurrentCrc
		{
			get
			{
				return this.currentCrc;
			}
		}

		public long CurrentCrcDataLength
		{
			get
			{
				return this.currentCrcDataLength;
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
				throw new NotSupportedException("CrcReaderStream does not support seeking");
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
				throw new NotSupportedException("CrcReaderStream does not support writing");
			}
			set
			{
				throw new NotSupportedException("CrcReaderStream does not support writing");
			}
		}

		public CrcReaderStream(Stream innerStream) : this(innerStream, true, 0, (long)0, (long)0, null)
		{
		}

		public CrcReaderStream(Stream innerStream, bool ownStream) : this(innerStream, ownStream, 0, (long)0, (long)0, null)
		{
		}

		public CrcReaderStream(Stream innerStream, bool ownStream, int maxCrcRangeSize) : this(innerStream, ownStream, maxCrcRangeSize, (long)0, (long)0, null)
		{
		}

		public CrcReaderStream(Stream innerStream, bool ownStream, int maxCrcRangeSize, long destinationOffset, long streamLength, RequestContext requestContext)
		{
			if (innerStream == null)
			{
				throw new ArgumentNullException("innerStream");
			}
			this.maxCrcRangeSize = maxCrcRangeSize;
			this.innerStream = innerStream;
			this.ownStream = ownStream;
			this.pageOffset = (int)(destinationOffset % (long)512);
			bool flag = false;
			this.crcCalcuationTimer = PerfTimerFactory.CreateTimer(flag, new TimeSpan((long)0));
			this.streamLength = streamLength;
			this.requestContext = requestContext;
		}

		public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			AsyncIteratorContext<int> asyncIteratorContext = new AsyncIteratorContext<int>("CrcReaderStream.Read", callback, state);
			asyncIteratorContext.Begin(this.ReadImpl(buffer, offset, count, asyncIteratorContext));
			return asyncIteratorContext;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
		{
			throw new NotSupportedException("BeginWrite is not supported by CrcReaderStream");
		}

		private void CheckDisposed()
		{
			if (this.innerStream == null)
			{
				throw new ObjectDisposedException("CrcReaderStream");
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
			throw new NotImplementedException("EndWrite is not supported by CrcReaderStream");
		}

		public override void Flush()
		{
			throw new NotSupportedException("Flush is not supported");
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			int num = this.EndRead(this.BeginRead(buffer, offset, count, null, null));
			return num;
		}

		private IEnumerator<IAsyncResult> ReadImpl(byte[] buffer, int offset, int count, AsyncIteratorContext<int> context)
		{
			bool flag;
			long num;
			this.CheckDisposed();
			IAsyncResult asyncResult = this.innerStream.BeginRead(buffer, offset, count, context.GetResumeCallback(), context.GetResumeState("CrcReaderStream.ReadImpl"));
			yield return asyncResult;
			int num1 = this.innerStream.EndRead(asyncResult);
			this.streamPosition += (long)num1;
			this.crcCalcuationTimer.Start();
			if (num1 > 0)
			{
				long num2 = (long)0;
				if (this.maxCrcRangeSize != 0)
				{
					if (this.crcArray == null)
					{
						this.crcArray = new List<long>(16);
					}
					long num3 = this.currentCrcDataLength;
					int num4 = num1;
					while (num4 > 0)
					{
						int num5 = this.maxCrcRangeSize;
						if (this.crcArray.Count <= 1 && num3 < (long)(this.maxCrcRangeSize - this.pageOffset))
						{
							num5 -= this.pageOffset;
						}
						int num6 = num5 - this.currentCrcRangeLength;
						if (num6 > num4)
						{
							num6 = num4;
						}
						long num7 = CrcUtils.ComputeCrc(buffer, offset, num6);
						num2 = (num3 <= (long)0 ? num7 : CrcUtils.ConcatenateCrc(num2, num3, num7, (long)num6));
						if (this.currentCrcRangeLength <= 0)
						{
							this.crcArray.Add(num7);
						}
						else
						{
							long item = this.crcArray[this.crcArray.Count - 1];
							item = CrcUtils.ConcatenateCrc(item, (long)this.currentCrcRangeLength, num7, (long)num6);
							this.crcArray[this.crcArray.Count - 1] = item;
						}
						num4 -= num6;
						offset += num6;
						num3 += (long)num6;
						this.currentCrcRangeLength += num6;
						if (this.currentCrcRangeLength != num5)
						{
							continue;
						}
						this.currentCrcRangeLength = 0;
					}
				}
				else
				{
					num2 = CrcUtils.ComputeCrc(buffer, offset, num1);
				}
				if (this.currentCrcDataLength <= (long)0)
				{
					this.currentCrc = num2;
				}
				else
				{
					this.currentCrc = CrcUtils.ConcatenateCrc(this.currentCrc, this.currentCrcDataLength, num2, (long)num1);
				}
				this.currentCrcDataLength += (long)num1;
			}
			this.crcCalcuationTimer.Stop();
			if (this.streamPosition == this.streamLength)
			{
				string base64String = Convert.ToBase64String(BitConverter.GetBytes(this.currentCrc));
				string str = this.Crc64ToVerifyAgainst.ToBase64String();
				if (this.requestContext != null)
				{
					this.requestContext.RequestContentCrc64 = str;
					this.requestContext.ServerContentCrc64 = base64String;
				}
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] objArray = new object[] { base64String, null, null };
				object[] objArray1 = objArray;
				string str1 = str;
				if (str1 == null)
				{
					str1 = "NULL";
				}
				objArray1[1] = str1;
				objArray[2] = this.currentCrcDataLength;
				verbose.Log("CalculatedCRC64 = {0} ReceivedCRC64 = {1} DataLength = {2}", objArray);
				if (this.Crc64ToVerifyAgainst.HasValue)
				{
					long? crc64ToVerifyAgainst = this.Crc64ToVerifyAgainst;
					long num8 = this.currentCrc;
					flag = (crc64ToVerifyAgainst.GetValueOrDefault() != num8 ? true : !crc64ToVerifyAgainst.HasValue);
					if (flag)
					{
						long? nullable = this.Crc64ToVerifyAgainst;
						num = (nullable.HasValue ? nullable.GetValueOrDefault() : (long)0);
						throw new CrcMismatchException("CrcReaderStream: Computed CRC64 does not match supplied CRC64", num, this.currentCrc, false);
					}
				}
			}
			this.calculatedCrc = this.currentCrc;
			context.ResultData = num1;
		}

		public void ResetCurrentCrc()
		{
			this.currentCrc = (long)0;
			this.currentCrcDataLength = (long)0;
			this.crcCalcuationTimer.Reset();
			this.currentCrcRangeLength = 0;
			if (this.crcArray != null)
			{
				this.crcArray.Clear();
			}
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotSupportedException("Seek is not supported by CrcReaderStream");
		}

		public override void SetLength(long value)
		{
			throw new NotSupportedException("SetLength is not supported by CrcReaderStream");
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotSupportedException("Write is not supported by CrcReaderStream");
		}
	}
}