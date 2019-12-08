using System;

namespace AsyncHelper
{
	internal interface IBufferManager
	{
		BufferWrapper GetBuffer(int minBufferSize);

		BufferWrapper GetExactBuffer(int powerOfTwoSize);

		void ReleaseBuffer(BufferWrapper bufferWrapper);
	}
}