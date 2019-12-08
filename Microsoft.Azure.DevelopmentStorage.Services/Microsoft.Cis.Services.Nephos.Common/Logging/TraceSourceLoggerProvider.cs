using System;
using System.Diagnostics;
using System.Globalization;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public class TraceSourceLoggerProvider : ILoggerProvider
	{
		private readonly TraceSource traceSource;

		private readonly object syncObj = new object();

		public TraceSourceLoggerProvider(string traceSourceName)
		{
			this.traceSource = new TraceSource(traceSourceName, SourceLevels.Information);
		}

		public Guid CreateActivityId(bool useDiscriminator)
		{
			return Guid.NewGuid();
		}

		public object GetLogger(Type loggerType, string name)
		{
			if (loggerType != typeof(IStringDataEventStream))
			{
				CultureInfo invariantCulture = CultureInfo.InvariantCulture;
				object[] objArray = new object[] { loggerType };
				throw new LoggerException(string.Format(invariantCulture, "The logger type {0} is not supported", objArray));
			}
			string str = name;
			string str1 = str;
			if (str != null)
			{
				if (str1 == "Error")
				{
					return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Error);
				}
				if (str1 == "Critical")
				{
					return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Critical);
				}
				if (str1 == "Warning")
				{
					return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Warning);
				}
				if (str1 == "Verbose")
				{
					return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Verbose);
				}
				if (str1 == "Info")
				{
					return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Information);
				}
			}
			return new TraceSourceLoggerProvider.StringDataEventStream(this, TraceEventType.Verbose);
		}

		private void TraceEvent(TraceEventType eventType, string message, params object[] args)
		{
			lock (this.syncObj)
			{
				this.traceSource.TraceEvent(eventType, 0, message, args);
			}
		}

		private class StringDataEventStream : IStringDataEventStream
		{
			private readonly TraceEventType eventType;

			private readonly TraceSourceLoggerProvider provider;

			public StringDataEventStream(TraceSourceLoggerProvider provider, TraceEventType eventType)
			{
				this.eventType = eventType;
				this.provider = provider;
			}

			public void DisableAllArchiving(bool disableArchiving)
			{
			}

			public void Flush()
			{
			}

			public void Log(string format, params object[] args)
			{
				this.provider.TraceEvent(this.eventType, format, args);
			}

			public void Log(string message)
			{
				this.provider.TraceEvent(this.eventType, message, new object[0]);
			}

			public void Log(int logId, string format, params object[] args)
			{
				this.Log(format, args);
			}
		}
	}
}