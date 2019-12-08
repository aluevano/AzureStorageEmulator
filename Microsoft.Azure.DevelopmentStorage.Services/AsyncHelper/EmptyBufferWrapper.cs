using System;

namespace AsyncHelper
{
	internal class EmptyBufferWrapper : BufferWrapper
	{
		public EmptyBufferWrapper() : base(new byte[0])
		{
		}

		internal override void ClearBuffer()
		{
		}
	}
}