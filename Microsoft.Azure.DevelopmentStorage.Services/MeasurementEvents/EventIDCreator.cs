using System;
using System.Threading;

namespace MeasurementEvents
{
	internal static class EventIDCreator
	{
		private static int currentId;

		internal static int Create()
		{
			int num = Interlocked.Increment(ref EventIDCreator.currentId) - 1;
			if (num < 0)
			{
				object[] objArray = new object[] { "The current AppDomain contains more than ", 2147483647, " event declarations.  This version of the event collection client can only support ", 2147483647, " event types." };
				throw new MeasurementEventException(string.Concat(objArray));
			}
			return num;
		}
	}
}