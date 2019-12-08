using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Anvil.RdUsage!TimeUtc", "27100", Justification="This is used for local logging.")]
	internal class DummyQuantileDataEventStream : IQuantileDataEventStream
	{
		public DummyQuantileDataEventStream()
		{
		}

		public void Log(long actionId, long status, long size, long durationInMS, string accountName, string containerName)
		{
		}

		public void Log(long actionId, long status, long size, long durationInMS, long processingTimeInMS, string accountName, string containerName)
		{
		}

		public void LogAudit(string userName, string clientAddress, int clientPort, string interfaceNameSpace, string interfaceName, string operation, string args, int result)
		{
		}
	}
}