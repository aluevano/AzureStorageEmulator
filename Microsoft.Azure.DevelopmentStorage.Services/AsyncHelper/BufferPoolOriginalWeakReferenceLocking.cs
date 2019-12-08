using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace AsyncHelper
{
	public static class BufferPoolOriginalWeakReferenceLocking
	{
		private static Dictionary<int, Stack<WeakReference>> buffers;

		static BufferPoolOriginalWeakReferenceLocking()
		{
			BufferPoolOriginalWeakReferenceLocking.buffers = new Dictionary<int, Stack<WeakReference>>();
		}

		private static BufferWrapper AllocateNewBuffer(int bufferSize)
		{
			return new BufferWrapper(new byte[bufferSize]);
		}

		public static BufferWrapper GetBuffer(int RoundUpToPowerOfTwoBufferSize)
		{
			BufferWrapper bufferWrapper;
			if (RoundUpToPowerOfTwoBufferSize == 0)
			{
				return BufferPool.EmptyByteBuffer;
			}
			lock (BufferPoolOriginalWeakReferenceLocking.buffers)
			{
				Stack<WeakReference> bufferStack = BufferPoolOriginalWeakReferenceLocking.GetBufferStack(RoundUpToPowerOfTwoBufferSize, BufferPoolOriginalWeakReferenceLocking.buffers);
				while (bufferStack.Count > 0)
				{
					byte[] target = (byte[])bufferStack.Pop().Target;
					if (target == null)
					{
						continue;
					}
					bufferWrapper = new BufferWrapper(target);
					return bufferWrapper;
				}
				bufferWrapper = BufferPoolOriginalWeakReferenceLocking.AllocateNewBuffer(RoundUpToPowerOfTwoBufferSize);
			}
			return bufferWrapper;
		}

		[SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId="syncObj", Justification="The lock is deliberately unused.  It is passed in to indicate that the lock is expected to be held when calling this function.")]
		private static Stack<WeakReference> GetBufferStack(int bufferSize, object syncObj)
		{
			Stack<WeakReference> weakReferences;
			if (!BufferPoolOriginalWeakReferenceLocking.buffers.TryGetValue(bufferSize, out weakReferences))
			{
				weakReferences = new Stack<WeakReference>();
				BufferPoolOriginalWeakReferenceLocking.buffers[bufferSize] = weakReferences;
			}
			return weakReferences;
		}

		public static void ReleaseBuffer(BufferWrapper bufferWrapper)
		{
			Stack<WeakReference> weakReferences;
			if (object.ReferenceEquals(bufferWrapper, BufferPool.EmptyByteBuffer))
			{
				return;
			}
			if (bufferWrapper.Buffer == null)
			{
				throw new InvalidOperationException("Buffer pointer is null which implies it is released twice to stack.");
			}
			byte[] buffer = bufferWrapper.Buffer;
			bufferWrapper.ClearBuffer();
			WeakReference weakReference = new WeakReference(buffer, false)
			{
				Target = buffer
			};
			int length = (int)buffer.Length;
			lock (BufferPoolOriginalWeakReferenceLocking.buffers)
			{
				if (BufferPoolOriginalWeakReferenceLocking.buffers.TryGetValue(length, out weakReferences))
				{
					weakReferences.Push(weakReference);
				}
			}
		}
	}
}