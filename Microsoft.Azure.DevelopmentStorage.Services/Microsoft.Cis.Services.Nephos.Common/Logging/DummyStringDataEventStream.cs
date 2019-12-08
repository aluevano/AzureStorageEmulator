using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Anvil.RdUsage!TimeUtc", "27100", Justification="This is used for local logging.")]
	internal class DummyStringDataEventStream : IStringDataEventStream
	{
		public DummyStringDataEventStream()
		{
		}

		public void DisableAllArchiving(bool disableArchiving)
		{
		}

		public void Flush()
		{
		}

		public void Log(string message)
		{
		}

		public void Log(string format, params object[] args)
		{
		}

		public void Log(int logId, string format, params object[] args)
		{
		}
	}
}