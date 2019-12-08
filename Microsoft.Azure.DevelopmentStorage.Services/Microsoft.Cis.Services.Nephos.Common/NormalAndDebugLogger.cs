using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	internal abstract class NormalAndDebugLogger : Logger<INormalAndDebugLogger>
	{
		private NormalAndDebugLogger()
		{
		}
	}
}