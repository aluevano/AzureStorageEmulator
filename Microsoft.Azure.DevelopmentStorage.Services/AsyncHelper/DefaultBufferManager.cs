using System;

namespace AsyncHelper
{
	internal class DefaultBufferManager : IBufferManager
	{
		public readonly static IBufferManager Instance;

		static DefaultBufferManager()
		{
			DefaultBufferManager.Instance = new DefaultBufferManager();
		}

		public DefaultBufferManager()
		{
		}

		public BufferWrapper GetBuffer(int minBufferSize)
		{
			return BufferPool.GetBuffer(minBufferSize);
		}

		public BufferWrapper GetExactBuffer(int powerOfTwoSize)
		{
			return BufferPool.GetExactBuffer(powerOfTwoSize);
		}

		public void ReleaseBuffer(BufferWrapper bufferWrapper)
		{
			BufferPool.ReleaseBuffer(bufferWrapper);
		}
	}
}