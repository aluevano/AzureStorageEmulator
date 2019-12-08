using AsyncHelper;
using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class PerformanceCounterWrapper
	{
		private readonly static string LocalStorageKey;

		private DateTime clockStart = DateTime.UtcNow;

		private DateTime clockEnd = DateTime.MinValue;

		private DateTime eventStart = DateTime.UtcNow;

		private readonly string title;

		private Dictionary<string, PerformanceCounterWrapper> counters = new Dictionary<string, PerformanceCounterWrapper>();

		private Dictionary<string, string> metadata = new Dictionary<string, string>();

		private Dictionary<string, long> events = new Dictionary<string, long>();

		private long totalTicks;

		public long ElapsedTicks
		{
			get
			{
				return DateTime.UtcNow.Ticks - this.eventStart.Ticks;
			}
		}

		private PerformanceCounterWrapper this[string counterName]
		{
			get
			{
				if (!this.counters.ContainsKey(counterName))
				{
					this.counters.Add(counterName, new PerformanceCounterWrapper(counterName));
				}
				return this.counters[counterName];
			}
		}

		public static PerformanceCounterWrapper LocalInstance
		{
			get
			{
				return (Trace.Metadata[PerformanceCounterWrapper.LocalStorageKey] as Stack<PerformanceCounterWrapper>).Peek();
			}
		}

		static PerformanceCounterWrapper()
		{
			PerformanceCounterWrapper.LocalStorageKey = Guid.NewGuid().ToString();
		}

		public PerformanceCounterWrapper(string title)
		{
			if (string.IsNullOrEmpty(title))
			{
				throw new ArgumentException("title");
			}
			this.title = title;
		}

		public void AddMetadata(string key, string val)
		{
			if (!this.metadata.ContainsKey(key))
			{
				this.metadata.Add(key, val);
				return;
			}
			Dictionary<string, string> strs = this.metadata;
			Dictionary<string, string> strs1 = strs;
			string str = key;
			string str1 = str;
			strs[str] = string.Concat(strs1[str1], val);
		}

		public void ClockIn(string eventName)
		{
			long elapsedTicks = this.ElapsedTicks;
			if (!this.events.ContainsKey(eventName))
			{
				this.events.Add(eventName, elapsedTicks);
			}
			else
			{
				Dictionary<string, long> item = this.events;
				Dictionary<string, long> strs = item;
				string str = eventName;
				item[str] = strs[str] + elapsedTicks;
			}
			this.eventStart = DateTime.UtcNow;
		}

		public static long ConvertTicksToMilliseconds(long ticks)
		{
			return ticks / (long)10000;
		}

		public static long ConvertTicksToNanoseconds(long ticks)
		{
			return ticks * (long)100;
		}

		public static void InitializeLocalPerformanceCounter(string title)
		{
			Stack<PerformanceCounterWrapper> performanceCounterWrappers = new Stack<PerformanceCounterWrapper>();
			performanceCounterWrappers.Push(new PerformanceCounterWrapper(title));
			Trace.Metadata[PerformanceCounterWrapper.LocalStorageKey] = performanceCounterWrappers;
		}

		public void Log()
		{
			Logger<IRestProtocolHeadLogger>.Instance.VerboseDebug.Log(this.ToString());
		}

		public void PopAsyncActivity()
		{
			PerformanceCounterWrapper.LocalInstance.StopClock();
			Stack<PerformanceCounterWrapper> item = Trace.Metadata[PerformanceCounterWrapper.LocalStorageKey] as Stack<PerformanceCounterWrapper>;
			item.Pop();
			item.Peek().ClockIn(this.title);
		}

		public void PushAsyncActivity(string activity)
		{
			Stack<PerformanceCounterWrapper> item = Trace.Metadata[PerformanceCounterWrapper.LocalStorageKey] as Stack<PerformanceCounterWrapper>;
			PerformanceCounterWrapper performanceCounterWrapper = this[activity];
			item.Push(performanceCounterWrapper);
			performanceCounterWrapper.ResetClock();
		}

		public void ResetClock()
		{
			DateTime utcNow = DateTime.UtcNow;
			DateTime dateTime = utcNow;
			this.eventStart = utcNow;
			this.clockStart = dateTime;
			this.clockEnd = DateTime.MinValue;
		}

		public void StopClock()
		{
			if (this.clockEnd != DateTime.MinValue)
			{
				return;
			}
			this.clockEnd = DateTime.UtcNow;
			PerformanceCounterWrapper ticks = this;
			ticks.totalTicks = ticks.totalTicks + (this.clockEnd.Ticks - this.clockStart.Ticks);
		}

		public override string ToString()
		{
			if (this.clockEnd == DateTime.MinValue)
			{
				this.StopClock();
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendFormat("name={0} total time={1}ms ", this.title, PerformanceCounterWrapper.ConvertTicksToMilliseconds(this.totalTicks));
			foreach (KeyValuePair<string, string> metadatum in this.metadata)
			{
				stringBuilder.AppendFormat("{0}={1} ", metadatum.Key, metadatum.Value);
			}
			foreach (KeyValuePair<string, long> @event in this.events)
			{
				stringBuilder.AppendFormat("{0}={1}ms({2}%) ", @event.Key, PerformanceCounterWrapper.ConvertTicksToMilliseconds(@event.Value), (this.totalTicks == (long)0 ? (long)0 : @event.Value * (long)100 / this.totalTicks));
			}
			foreach (KeyValuePair<string, PerformanceCounterWrapper> counter in this.counters)
			{
				stringBuilder.AppendFormat("[ {0} ] ", counter.Value.ToString());
			}
			return stringBuilder.ToString();
		}
	}
}