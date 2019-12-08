using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace AsyncHelper
{
	[DebuggerStepThrough]
	public static class Trace
	{
		private const uint EVENT_ACTIVITY_CTRL_GET_ID = 1;

		private const uint EVENT_ACTIVITY_CTRL_SET_ID = 2;

		private const uint EVENT_ACTIVITY_CTRL_CREATE_ID = 3;

		private const uint EVENT_ACTIVITY_CTRL_GET_SET_ID = 4;

		private const uint EVENT_ACTIVITY_CTRL_CREATE_SET_ID = 5;

		private readonly static bool EventActivityIdControlIsAvailable;

		[ThreadStatic]
		private static Dictionary<string, object> metadata;

		public static Guid ActivityId
		{
			get
			{
				return System.Diagnostics.Trace.CorrelationManager.ActivityId;
			}
			set
			{
				System.Diagnostics.Trace.CorrelationManager.ActivityId = value;
				if (AsyncHelper.Trace.EventActivityIdControlIsAvailable)
				{
					AsyncHelper.Trace.NativeMethods.EventActivityIdControl(2, ref value);
				}
			}
		}

		public static Dictionary<string, object> Metadata
		{
			get
			{
				return AsyncHelper.Trace.metadata;
			}
			set
			{
				AsyncHelper.Trace.metadata = value;
			}
		}

		static Trace()
		{
			try
			{
				Guid guid = new Guid();
				AsyncHelper.Trace.NativeMethods.EventActivityIdControl(1, ref guid);
				AsyncHelper.Trace.EventActivityIdControlIsAvailable = true;
			}
			catch (EntryPointNotFoundException entryPointNotFoundException)
			{
				AsyncHelper.Trace.EventActivityIdControlIsAvailable = false;
			}
		}

		private static class NativeMethods
		{
			[DllImport("advapi32.dll", CharSet=CharSet.Unicode, ExactSpelling=false)]
			public static extern uint EventActivityIdControl(uint controlCode, ref Guid activityId);
		}
	}
}