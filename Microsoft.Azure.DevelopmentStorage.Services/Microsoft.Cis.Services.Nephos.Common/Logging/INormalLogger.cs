namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface INormalLogger
	{
		[EventStream]
		IStringDataEventStream Critical
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Error
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Event
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Info
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Perf
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Status
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Verbose
		{
			get;
		}

		[EventStream]
		IStringDataEventStream Warning
		{
			get;
		}
	}
}