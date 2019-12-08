using System;

namespace AsyncHelper
{
	public static class BufferPool
	{
		public const int BufferPoolBucketSize = 33;

		public const long KiloByte = 1024L;

		public const long MegaByte = 1048576L;

		public const long GigaByte = 1073741824L;

		internal static BufferWrapper EmptyByteBuffer;

		static BufferPool()
		{
			BufferPool.EmptyByteBuffer = new EmptyBufferWrapper();
		}

		public static BufferWrapper GetBuffer(int minBufferSize)
		{
			if (minBufferSize == 0)
			{
				return BufferPool.EmptyByteBuffer;
			}
			if (minBufferSize < 0)
			{
				throw new ArgumentOutOfRangeException("bufferSize", (object)minBufferSize, "The buffer size must not be negative.");
			}
			return BufferPoolOriginalWeakReferenceLocking.GetBuffer(BufferPool.RoundUpToPowerOfTwo(minBufferSize));
		}

		internal static BufferWrapper GetExactBuffer(int powerOfTwoSize)
		{
			if (powerOfTwoSize == 0)
			{
				return BufferPool.EmptyByteBuffer;
			}
			if (powerOfTwoSize < 0 || !BufferPool.IsPowerOfTwo(powerOfTwoSize))
			{
				throw new ArgumentOutOfRangeException("powerOfTwoSize", "The buffer size must be a positive number that is a power of two");
			}
			return BufferPoolOriginalWeakReferenceLocking.GetBuffer(powerOfTwoSize);
		}

		public static int GetIndex(int bufferSize)
		{
			if (bufferSize < 0)
			{
				throw new InvalidOperationException(string.Format("buffer size is invalid negative number:{0}", bufferSize));
			}
			if (bufferSize == 0)
			{
				return 0;
			}
			int num = 1;
			while ((bufferSize & 1) == 0 && num < 33)
			{
				num++;
				bufferSize >>= 1;
			}
			if (num >= 33)
			{
				throw new InvalidOperationException(string.Format("buffer size is invalid :{0}", bufferSize));
			}
			return num;
		}

		private static bool IsPowerOfTwo(int number)
		{
			if (number == 0)
			{
				return false;
			}
			return (number & number - 1) == 0;
		}

		public static void ReleaseBuffer(BufferWrapper bufferWrapper)
		{
			if (object.ReferenceEquals(bufferWrapper, BufferPool.EmptyByteBuffer))
			{
				return;
			}
			if (bufferWrapper.Buffer == null)
			{
				throw new InvalidOperationException("Buffer pointer is null which implies it is released twice to stack.");
			}
			if (((int)bufferWrapper.Buffer.Length & (int)bufferWrapper.Buffer.Length - 1) != 0)
			{
				return;
			}
			BufferPoolOriginalWeakReferenceLocking.ReleaseBuffer(bufferWrapper);
		}

		public static int RoundUpToPowerOfTwo(int number)
		{
			number--;
			number = number | number >> 1;
			number = number | number >> 2;
			number = number | number >> 4;
			number = number | number >> 8;
			number = number | number >> 16;
			number++;
			if (number < 0)
			{
				number = 2147483647;
			}
			return number;
		}
	}
}