using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Anvil.RdUsage!TimeUtc", "27100", Justification="This is used for local logging.")]
	internal class ConsoleQuantileDataEventStream : IQuantileDataEventStream
	{
		private string eventStreamName;

		public ConsoleQuantileDataEventStream(string eventStreamName)
		{
			if (eventStreamName == null)
			{
				throw new ArgumentNullException("eventStreamName");
			}
			this.eventStreamName = eventStreamName;
		}

		public void Log(long actionId, long status, long size, long durationInMS, string accountName, string containerName)
		{
			object[] now = new object[] { DateTime.Now, this.eventStreamName, actionId, status, size, durationInMS, accountName, containerName };
			Console.WriteLine("{0} [{1}] actionId={2}, status={3}, size={4}, durationInMs={5}, accountName={6}, containerName={7}", now);
		}

		public void Log(long actionId, long status, long size, long durationInMS, long processingTimeInMs, string accountName, string containerName)
		{
			object[] now = new object[] { DateTime.Now, this.eventStreamName, actionId, status, size, durationInMS, processingTimeInMs, accountName, containerName };
			Console.WriteLine("{0} [{1}] actionId={2}, status={3}, size={4}, durationInMs={5}, processingTimeInMs={6} accountName={7}, containerName={8}", now);
		}

		public void LogAudit(string userName, string clientAddress, int clientPort, string interfaceNameSpace, string interfaceName, string operation, string args, int result)
		{
			object[] objArray = new object[] { userName, clientAddress, clientPort, interfaceNameSpace, interfaceName, operation, args, result };
			Console.WriteLine("Auditing userName={0} clientAddress={1} clientPort={2}, interfaceNameSpace={3}, interfaceName={4}, operation={5}, args={6} result={7}", objArray);
		}
	}
}