using Microsoft.Cis.Services.Nephos.Common.Logging;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IRestProtocolHeadLogger : INormalAndDebugLogger, INormalLogger, IDebugLogger
	{
		[EventStream]
		IStringDataEventStream AuthenticationFailure
		{
			get;
		}

		[EventStream]
		IStringDataEventStream AuthorizationFailure
		{
			get;
		}

		[EventStream]
		IStringDataEventStream NetworkFailure
		{
			get;
		}

		[EventStream]
		IStringDataEventStream RequestTimeout
		{
			get;
		}

		[EventStream]
		IStringDataEventStream UnexpectedXStoreError
		{
			get;
		}

		[EventStream]
		IStringDataEventStream UnhandledException
		{
			get;
		}
	}
}