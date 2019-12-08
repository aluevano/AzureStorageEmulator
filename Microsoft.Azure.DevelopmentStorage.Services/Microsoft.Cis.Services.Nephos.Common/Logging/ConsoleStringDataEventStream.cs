using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	[SuppressMessage("Anvil.RdUsage!TimeUtc", "27100", Justification="This is used for local logging.")]
	internal class ConsoleStringDataEventStream : IStringDataEventStream
	{
		private string eventStreamName;

		public ConsoleStringDataEventStream(string eventStreamName)
		{
			if (eventStreamName == null)
			{
				throw new ArgumentNullException("eventStreamName");
			}
			this.eventStreamName = eventStreamName;
		}

		public void DisableAllArchiving(bool disableArchiving)
		{
		}

		public void Flush()
		{
		}

		public void Log(string message)
		{
			Console.WriteLine("{0} [{1}] {2}", DateTime.Now, this.eventStreamName, message);
		}

		public void Log(string format, params object[] args)
		{
			Console.WriteLine("{0} [{1}] {2}", DateTime.Now, this.eventStreamName, string.Format(CultureInfo.InvariantCulture, format, args));
		}

		public void Log(int logId, string format, params object[] args)
		{
			this.Log(format, args);
		}
	}
}