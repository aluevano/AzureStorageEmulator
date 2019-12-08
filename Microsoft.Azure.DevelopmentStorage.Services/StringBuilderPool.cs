using System;
using System.Collections.Generic;
using System.Text;

public class StringBuilderPool
{
	private static Stack<StringBuilder> StringBuilderStack;

	private static int TotalAllocated;

	private static int PeakInStack;

	static StringBuilderPool()
	{
		StringBuilderPool.StringBuilderStack = new Stack<StringBuilder>();
		StringBuilderPool.TotalAllocated = 0;
		StringBuilderPool.PeakInStack = 0;
	}

	public StringBuilderPool()
	{
	}

	public static StringBuilder Allocate()
	{
		StringBuilder stringBuilder;
		lock (StringBuilderPool.StringBuilderStack)
		{
			if (StringBuilderPool.StringBuilderStack.Count <= 0)
			{
				StringBuilderPool.TotalAllocated++;
				return new StringBuilder(1024);
			}
			else
			{
				stringBuilder = StringBuilderPool.StringBuilderStack.Pop();
			}
		}
		return stringBuilder;
	}

	public static void Release(StringBuilder builder)
	{
		if (builder == null)
		{
			return;
		}
		builder.Length = 0;
		lock (StringBuilderPool.StringBuilderStack)
		{
			StringBuilderPool.StringBuilderStack.Push(builder);
		}
		int count = StringBuilderPool.StringBuilderStack.Count;
		if (count > StringBuilderPool.PeakInStack)
		{
			StringBuilderPool.PeakInStack = count;
		}
	}
}