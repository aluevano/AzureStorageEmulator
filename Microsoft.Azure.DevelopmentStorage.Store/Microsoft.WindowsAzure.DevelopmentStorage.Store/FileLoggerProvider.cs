using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Globalization;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	public class FileLoggerProvider : ILoggerProvider
	{
		public FileLoggerProvider()
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
				return new FileStringDataEventStream(name);
			}
			if (loggerType != typeof(IQuantileDataEventStream))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { loggerType };
				throw new LoggerException(string.Format(invariantCulture, "The type {0} is not supported", objArray));
			}
			return new FileQuantileDataEventStream(name);
		}
	}
}