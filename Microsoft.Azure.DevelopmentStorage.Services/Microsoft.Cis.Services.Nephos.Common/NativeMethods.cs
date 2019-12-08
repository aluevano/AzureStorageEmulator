using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public static class NativeMethods
	{
		[DllImport("winmm.dll", CharSet=CharSet.None, ExactSpelling=false)]
		[SuppressMessage("Microsoft.Interoperability", "CA1401")]
		public static extern int timeBeginPeriod(int uMilliseconds);

		[DllImport("winmm.dll", CharSet=CharSet.None, ExactSpelling=false)]
		[SuppressMessage("Microsoft.Interoperability", "CA1401")]
		public static extern uint timeGetTime();
	}
}