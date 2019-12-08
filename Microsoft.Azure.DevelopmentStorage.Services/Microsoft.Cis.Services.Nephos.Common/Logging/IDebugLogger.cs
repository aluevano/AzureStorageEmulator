namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IDebugLogger
	{
		[EventStream("Error_debug")]
		IStringDataEventStream ErrorDebug
		{
			get;
		}

		[EventStream("Info_debug")]
		IStringDataEventStream InfoDebug
		{
			get;
		}

		[EventStream("Verbose_debug")]
		IStringDataEventStream VerboseDebug
		{
			get;
		}

		[EventStream("Warning_debug")]
		IStringDataEventStream WarningDebug
		{
			get;
		}
	}
}