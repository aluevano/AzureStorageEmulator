using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
	public interface IStringDataEventStream
	{
		void DisableAllArchiving(bool disableArchiving);

		void Flush();

		void Log(string message);

		void Log(string format, params object[] args);

		void Log(int logId, string format, params object[] args);
	}
}