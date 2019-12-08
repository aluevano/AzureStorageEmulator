using System;
using System.Diagnostics;

namespace AsyncHelper
{
	internal static class Tracing
	{
		public readonly static TraceSourceAdapter TraceSource;

		static Tracing()
		{
			Tracing.TraceSource = new TraceSourceAdapter("AsyncHelper", SourceLevels.Off);
		}
	}
}