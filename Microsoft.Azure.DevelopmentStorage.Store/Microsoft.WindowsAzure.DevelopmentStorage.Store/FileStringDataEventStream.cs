using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace Microsoft.WindowsAzure.DevelopmentStorage.Store
{
	internal class FileStringDataEventStream : IStringDataEventStream
	{
		private string eventStreamName;

		public FileStringDataEventStream(string eventStreamName)
		{
			if (eventStreamName == null)
			{
				throw new ArgumentNullException("eventStreamName");
			}
			this.eventStreamName = eventStreamName;
			if (System.Diagnostics.Trace.Listeners[eventStreamName] == null)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(DevelopmentStorageDbDataContext.LogDirectory);
				try
				{
					if (!directoryInfo.Exists)
					{
						directoryInfo.Create();
					}
					TraceListenerCollection listeners = System.Diagnostics.Trace.Listeners;
					string[] fullName = new string[] { directoryInfo.FullName, "\\", eventStreamName, null, null };
					fullName[3] = DateTime.Now.ToString("dd-MMM-yy-HH-MM");
					fullName[4] = ".log";
					listeners.Add(new TextWriterTraceListener(string.Concat(fullName), eventStreamName));
					System.Diagnostics.Trace.AutoFlush = true;
				}
				catch (Exception exception)
				{
					throw new Exception(string.Format("Log Folder creation Failed:{0}", exception.Message));
				}
			}
		}

		public void DisableAllArchiving(bool disableArchiving)
		{
		}

		public void Flush()
		{
		}

		public void Log(string message)
		{
			lock (System.Diagnostics.Trace.Listeners[this.eventStreamName])
			{
				TraceListener item = System.Diagnostics.Trace.Listeners[this.eventStreamName];
				object[] now = new object[] { DateTime.Now, this.eventStreamName, AsyncHelper.Trace.ActivityId, message };
				item.WriteLine(string.Format("{0} [{1}] [ActivityId={2}] {3}", now));
				System.Diagnostics.Trace.Listeners[this.eventStreamName].Flush();
			}
		}

		public void Log(string format, params object[] args)
		{
			string str = string.Format(CultureInfo.InvariantCulture, format, args);
			this.Log(str);
		}

		public void Log(int logId, string format, params object[] args)
		{
			this.Log(format, args);
		}
	}
}