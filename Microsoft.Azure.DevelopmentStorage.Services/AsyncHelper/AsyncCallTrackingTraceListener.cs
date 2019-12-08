using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;

namespace AsyncHelper
{
	public class AsyncCallTrackingTraceListener : TraceListener
	{
		private static Dictionary<AsyncIteratorContextBase, AsyncCallTrackingTraceListener.AsyncIteratorState> iterators;

		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification="this property is intended to be called from a debugger.  It should not throw, as this makes debugging difficult.")]
		public static string CurrentCallTree
		{
			get
			{
				string str;
				try
				{
					using (MemoryStream memoryStream = new MemoryStream())
					{
						AsyncCallTrackingTraceListener.BuildCallTree(memoryStream);
						str = Encoding.UTF8.GetString(memoryStream.ToArray());
					}
				}
				catch (Exception exception)
				{
					str = string.Concat("Exception while generating async call tree:\n", exception.ToString());
				}
				return str;
			}
		}

		public override bool IsThreadSafe
		{
			get
			{
				return true;
			}
		}

		static AsyncCallTrackingTraceListener()
		{
			AsyncCallTrackingTraceListener.iterators = new Dictionary<AsyncIteratorContextBase, AsyncCallTrackingTraceListener.AsyncIteratorState>();
		}

		public AsyncCallTrackingTraceListener() : base("AsyncCallTrackingTraceListener")
		{
		}

		private static void AppendCallTree(DateTime now, XmlTextWriter writer, AsyncCallTrackingTraceListener.AsyncIteratorState methodState, Dictionary<AsyncCallTrackingTraceListener.AsyncIteratorState, List<AsyncCallTrackingTraceListener.AsyncIteratorState>> allCalls)
		{
			List<AsyncCallTrackingTraceListener.AsyncIteratorState> asyncIteratorStates;
			writer.WriteStartElement("method");
			writer.WriteStartAttribute("name");
			writer.WriteString(methodState.Iterator.MethodName);
			writer.WriteEndAttribute();
			if (methodState.Call != null)
			{
				TimeSpan timeSpan = now - methodState.SuspendedTime;
				writer.WriteStartAttribute("suspendedMilliseconds");
				writer.WriteValue(timeSpan.TotalMilliseconds);
				writer.WriteEndAttribute();
			}
			else
			{
				writer.WriteStartAttribute("runningOnThread");
				writer.WriteString(methodState.ThreadId);
				writer.WriteEndAttribute();
			}
			if (allCalls.TryGetValue(methodState, out asyncIteratorStates))
			{
				foreach (AsyncCallTrackingTraceListener.AsyncIteratorState asyncIteratorState in asyncIteratorStates)
				{
					writer.WriteStartElement("call");
					writer.WriteStartAttribute("name");
					writer.WriteString(asyncIteratorState.Iterator.Caller.CalleeName);
					writer.WriteEndAttribute();
					if (object.ReferenceEquals(asyncIteratorState.Iterator.Caller, methodState.Call))
					{
						writer.WriteStartAttribute("waiting");
						writer.WriteValue(true);
						writer.WriteEndAttribute();
					}
					AsyncCallTrackingTraceListener.AppendCallTree(now, writer, asyncIteratorState, allCalls);
					writer.WriteEndElement();
				}
			}
			writer.WriteEndElement();
		}

		private static void BuildCallTree(Stream stream)
		{
			List<AsyncCallTrackingTraceListener.AsyncIteratorState> asyncIteratorStates;
			AsyncCallTrackingTraceListener.AsyncIteratorState asyncIteratorState;
			List<AsyncCallTrackingTraceListener.AsyncIteratorState> asyncIteratorStates1 = new List<AsyncCallTrackingTraceListener.AsyncIteratorState>();
			Dictionary<AsyncCallTrackingTraceListener.AsyncIteratorState, List<AsyncCallTrackingTraceListener.AsyncIteratorState>> asyncIteratorStates2 = new Dictionary<AsyncCallTrackingTraceListener.AsyncIteratorState, List<AsyncCallTrackingTraceListener.AsyncIteratorState>>();
			lock (AsyncCallTrackingTraceListener.iterators)
			{
				DateTime utcNow = DateTime.UtcNow;
				foreach (AsyncCallTrackingTraceListener.AsyncIteratorState value in AsyncCallTrackingTraceListener.iterators.Values)
				{
					AsyncCallTracker caller = value.Iterator.Caller;
					if (caller != null)
					{
						if (!AsyncCallTrackingTraceListener.iterators.TryGetValue(caller.Caller, out asyncIteratorState))
						{
							continue;
						}
						if (!asyncIteratorStates2.TryGetValue(asyncIteratorState, out asyncIteratorStates))
						{
							asyncIteratorStates = new List<AsyncCallTrackingTraceListener.AsyncIteratorState>(1);
							asyncIteratorStates2.Add(asyncIteratorState, asyncIteratorStates);
						}
						asyncIteratorStates.Add(value);
					}
					else
					{
						asyncIteratorStates1.Add(value);
					}
				}
				using (XmlTextWriter xmlTextWriter = new XmlTextWriter(stream, Encoding.UTF8))
				{
					xmlTextWriter.Formatting = Formatting.Indented;
					xmlTextWriter.WriteStartDocument();
					xmlTextWriter.WriteStartElement("callTree");
					xmlTextWriter.WriteStartAttribute("time");
					xmlTextWriter.WriteValue(utcNow);
					xmlTextWriter.WriteEndAttribute();
					foreach (AsyncCallTrackingTraceListener.AsyncIteratorState asyncIteratorState1 in asyncIteratorStates1)
					{
						AsyncCallTrackingTraceListener.AppendCallTree(utcNow, xmlTextWriter, asyncIteratorState1, asyncIteratorStates2);
					}
					xmlTextWriter.WriteEndElement();
					xmlTextWriter.WriteEndDocument();
				}
			}
		}

		public static void StaticTraceData(TraceEventCache eventCache, string source, TraceEventType eventType, object data)
		{
			AsyncCallTrackingTraceListener.AsyncIteratorState asyncIteratorState;
			if (source != "AsyncHelper")
			{
				return;
			}
			AsyncIteratorContextBase asyncIteratorContextBase = data as AsyncIteratorContextBase;
			TraceEventType traceEventType = eventType;
			if (traceEventType <= TraceEventType.Stop)
			{
				if (traceEventType == TraceEventType.Start)
				{
					lock (AsyncCallTrackingTraceListener.iterators)
					{
						asyncIteratorState = new AsyncCallTrackingTraceListener.AsyncIteratorState()
						{
							Iterator = asyncIteratorContextBase,
							ThreadId = eventCache.ThreadId
						};
						AsyncCallTrackingTraceListener.iterators.Add(asyncIteratorState.Iterator, asyncIteratorState);
					}
				}
				else
				{
					if (traceEventType != TraceEventType.Stop)
					{
						return;
					}
					lock (AsyncCallTrackingTraceListener.iterators)
					{
						AsyncCallTrackingTraceListener.iterators.Remove(asyncIteratorContextBase);
					}
				}
			}
			else if (traceEventType == TraceEventType.Suspend)
			{
				lock (AsyncCallTrackingTraceListener.iterators)
				{
					AsyncCallTracker asyncCallTracker = (AsyncCallTracker)data;
					asyncIteratorState = AsyncCallTrackingTraceListener.iterators[asyncCallTracker.Caller];
					asyncIteratorState.Call = asyncCallTracker;
					asyncIteratorState.SuspendedTime = DateTime.UtcNow;
				}
			}
			else
			{
				if (traceEventType != TraceEventType.Resume)
				{
					return;
				}
				lock (AsyncCallTrackingTraceListener.iterators)
				{
					asyncIteratorState = AsyncCallTrackingTraceListener.iterators[asyncIteratorContextBase];
					asyncIteratorState.Call = null;
					asyncIteratorState.ThreadId = eventCache.ThreadId;
				}
			}
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			AsyncCallTrackingTraceListener.StaticTraceData(eventCache, source, eventType, data);
		}

		public override void Write(string message)
		{
		}

		public override void WriteLine(string message)
		{
		}

		private class AsyncIteratorState
		{
			private AsyncIteratorContextBase iterator;

			private AsyncCallTracker call;

			private DateTime suspendedTime;

			private string threadId;

			public AsyncCallTracker Call
			{
				get
				{
					return this.call;
				}
				set
				{
					this.call = value;
				}
			}

			public AsyncIteratorContextBase Iterator
			{
				get
				{
					return this.iterator;
				}
				set
				{
					this.iterator = value;
				}
			}

			public DateTime SuspendedTime
			{
				get
				{
					return this.suspendedTime;
				}
				set
				{
					this.suspendedTime = value;
				}
			}

			public string ThreadId
			{
				get
				{
					return this.threadId;
				}
				set
				{
					this.threadId = value;
				}
			}

			public AsyncIteratorState()
			{
			}
		}
	}
}