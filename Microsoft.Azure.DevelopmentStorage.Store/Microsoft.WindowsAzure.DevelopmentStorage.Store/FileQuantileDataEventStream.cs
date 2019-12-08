using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class FileQuantileDataEventStream : IQuantileDataEventStream
	{
		private string eventStreamName;

		public FileQuantileDataEventStream(string eventStreamName)
		{
			if (eventStreamName == null)
			{
				throw new ArgumentNullException("eventStreamName");
			}
			this.eventStreamName = eventStreamName;
		}

		public void Log(ulong actionId, ulong status, ulong size, ulong durationInMS, string accountName, string containerName)
		{
			object[] now = new object[] { DateTime.Now, this.eventStreamName, actionId, status, size, durationInMS, accountName, containerName };
			Console.WriteLine("{0} [{1}] actionId={2}, status={3}, size={4}, durationInMs={5}, accountName={6}, containerName={7}", now);
		}

		public void Log(long actionId, long status, long size, long durationInMS, string accountName, string containerName)
		{
			throw new NotImplementedException();
		}

		public void Log(long actionId, long status, long size, long durationInMS, long processintTimeInMs, string accountName, string containerName)
		{
			throw new NotImplementedException();
		}

		public void LogAudit(string userName, string clientAddress, int clientPort, string interfaceNameSpace, string interfaceName, string operation, string args, int result)
		{
			throw new NotImplementedException();
		}
	}
}