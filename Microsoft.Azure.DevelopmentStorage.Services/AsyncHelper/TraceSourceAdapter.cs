using System;
using System.Diagnostics;

namespace AsyncHelper
{
	internal class TraceSourceAdapter
	{
		public static bool TracingEnabled;

		private readonly TraceSource _traceSource;

		static TraceSourceAdapter()
		{
		}

		public TraceSourceAdapter(string name, SourceLevels defaultLevel)
		{
			this._traceSource = new TraceSource(name, defaultLevel);
		}

		public void TraceData(TraceEventType eventType, int id, object data)
		{
			if (TraceSourceAdapter.TracingEnabled)
			{
				this._traceSource.TraceData(eventType, id, data);
			}
		}

		public void TraceEvent(TraceEventType eventType, int id, string message)
		{
			if (TraceSourceAdapter.TracingEnabled)
			{
				this._traceSource.TraceData(eventType, id, message);
			}
		}
	}
}