using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public static class LoggerProvider
	{
		private static object syncObj;

		private static ILoggerProvider instance;

		public static ILoggerProvider Instance
		{
			get
			{
				ILoggerProvider loggerProvider;
				lock (LoggerProvider.syncObj)
				{
					loggerProvider = LoggerProvider.instance;
				}
				return loggerProvider;
			}
			set
			{
				lock (LoggerProvider.syncObj)
				{
					LoggerProvider.instance = value;
				}
			}
		}

		static LoggerProvider()
		{
			LoggerProvider.syncObj = new object();
		}
	}
}