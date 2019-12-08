using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IQuantileDataEventStream
	{
		void Log(long actionId, long status, long size, long durationInMS, string accountName, string containerName);

		void Log(long actionId, long status, long size, long durationInMS, long processingTimeInMS, string accountName, string containerName);

		void LogAudit(string userName, string clientAddress, int clientPort, string interfaceNameSpace, string interfaceName, string operation, string args, int result);
	}
}