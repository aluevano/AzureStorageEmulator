using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public static class EndToEndLogging
	{
		private const string ActivityTypeStart = "ActivityBegin";

		private const string ActivityTypeEnd = "ActivityEnd";

		private const string ActivityNameString = "ActivityName";

		public static void LogActivityEnd(IStringDataEventStream eventStream, string activityName)
		{
			object[] objArray = new object[] { "ActivityEnd", "ActivityName", activityName };
			eventStream.Log("{0}, {1}={2}", objArray);
		}

		public static void LogActivityStart(IStringDataEventStream eventStream, string activityName)
		{
			object[] objArray = new object[] { "ActivityBegin", "ActivityName", activityName };
			eventStream.Log("{0}, {1}={2} CorrelationInfo=", objArray);
		}
	}
}