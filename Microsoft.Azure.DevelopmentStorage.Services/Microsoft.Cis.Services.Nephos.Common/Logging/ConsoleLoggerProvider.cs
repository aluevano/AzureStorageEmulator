using System;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public class ConsoleLoggerProvider : ILoggerProvider
	{
		public ConsoleLoggerProvider()
		{
		}

		public Guid CreateActivityId(bool useDiscriminator)
		{
			return Guid.NewGuid();
		}

		public object GetLogger(Type loggerType, string name)
		{
			if (loggerType == typeof(IStringDataEventStream))
			{
				return new ConsoleStringDataEventStream(name);
			}
			if (loggerType != typeof(IQuantileDataEventStream))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { loggerType };
				throw new LoggerException(string.Format(invariantCulture, "The type {0} is not supported", objArray));
			}
			return new ConsoleQuantileDataEventStream(name);
		}
	}
}