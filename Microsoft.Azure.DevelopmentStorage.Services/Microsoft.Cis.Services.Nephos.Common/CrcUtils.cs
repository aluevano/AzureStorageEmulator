using Microsoft.Cis.Services.Nephos.Common.Streams;
using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class CrcUtils
	{
		public static long ComputeCrc(byte[] inputData, int offset, int count)
		{
			return (long)-1;
		}

		public static long ConcatenateCrc(long crcA, long sizeA, long crcB, long sizeB)
		{
			return (long)-1;
		}

		public static void SplitCalculateDataCrc(int splitIndex, int blockSize, byte[] data, long expectedChunkCrc, out long crc1, out long crc2)
		{
			crc1 = CrcUtils.ComputeCrc(data, 0, splitIndex + 1);
			int num = blockSize - splitIndex - 1;
			crc2 = CrcUtils.ComputeCrc(data, splitIndex + 1, num);
			long num1 = CrcUtils.ConcatenateCrc(crc1, (long)(splitIndex + 1), crc2, (long)num);
			if (num1 != expectedChunkCrc)
			{
				throw new CrcMismatchException("Hit CRC mismatch while calculating data CRCs and splitting.", expectedChunkCrc, num1, true);
			}
		}

		public static void SplitCalculateDataCrc(int splitIndex1, int splitIndex2, int blockSize, byte[] data, long expectedChunkCrc, out long crc1, out long crc2, out long crc3)
		{
			int num = splitIndex1 + 1;
			crc1 = CrcUtils.ComputeCrc(data, 0, num);
			int num1 = splitIndex2 - splitIndex1;
			crc2 = CrcUtils.ComputeCrc(data, splitIndex1 + 1, num1);
			int num2 = blockSize - splitIndex2 - 1;
			crc3 = CrcUtils.ComputeCrc(data, splitIndex2 + 1, num2);
			long num3 = CrcUtils.ConcatenateCrc(crc1, (long)num, crc2, (long)num1);
			num3 = CrcUtils.ConcatenateCrc(num3, (long)(num + num1), crc3, (long)num2);
			if (num3 != expectedChunkCrc)
			{
				throw new CrcMismatchException("Hit CRC mismatch while calculating data CRCs and splitting in 3 ways.", expectedChunkCrc, num3, true);
			}
		}
	}
}