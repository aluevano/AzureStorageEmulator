using Microsoft.Cis.Services.Nephos.Common;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.IO;

namespace Microsoft.Cis.Services.Nephos.Common.Streams
{
	public class CrcVerifierStream : CrcStream
	{
		private readonly Stream innerStream;

		private readonly bool ownsInnerStream;

		public override bool CanRead
		{
			get
			{
				return this.innerStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return this.innerStream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return this.innerStream.CanWrite;
			}
		}

		public override long Length
		{
			get
			{
				return this.innerStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return this.innerStream.Position;
			}
			set
			{
				this.innerStream.Position = value;
			}
		}

		public CrcVerifierStream(Stream innerStream, bool ownsInnerStream)
		{
			this.innerStream = innerStream;
			this.ownsInnerStream = ownsInnerStream;
		}

		public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, long? crc, AsyncCallback callback, object state)
		{
			this.VerifyCrc(buffer, offset, count, crc);
			return this.innerStream.BeginWrite(buffer, offset, count, callback, state);
		}

		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && this.ownsInnerStream)
				{
					this.innerStream.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		public override void EndWrite(IAsyncResult asyncResult)
		{
			this.innerStream.EndWrite(asyncResult);
		}

		public override void Flush()
		{
			this.innerStream.Flush();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			throw new NotImplementedException();
		}

		public override void SetLength(long value)
		{
			throw new NotImplementedException();
		}

		private void VerifyCrc(byte[] buffer, int offset, int count, long? crc)
		{
			if (crc.HasValue)
			{
				long num = CrcUtils.ComputeCrc(buffer, offset, count);
				if (num != crc.Value)
				{
					throw new CrcMismatchException("Calculated CRC does not match Read CRC while streaming data to client", crc.Value, num, true);
				}
				IStringDataEventStream verbose = Logger<IRestProtocolHeadLogger>.Instance.Verbose;
				object[] objArray = new object[] { count, num };
				verbose.Log("Data CRC while streaming to client. Length:{0} CRC:0x{1:x}", objArray);
			}
		}

		public override void Write(byte[] buffer, int offset, int count, long? crc)
		{
			this.VerifyCrc(buffer, offset, count, crc);
			this.innerStream.Write(buffer, offset, count);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			throw new NotImplementedException();
		}
	}
}