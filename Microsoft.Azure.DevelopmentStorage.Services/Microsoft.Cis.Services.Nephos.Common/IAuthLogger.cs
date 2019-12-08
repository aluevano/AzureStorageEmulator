using Microsoft.Cis.Services.Nephos.Common.Logging;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IAuthLogger : INormalAndDebugLogger, INormalLogger, IDebugLogger
	{
		[EventStream]
		IStringDataEventStream AuthenticationFailure
		{
			get;
		}
	}
}