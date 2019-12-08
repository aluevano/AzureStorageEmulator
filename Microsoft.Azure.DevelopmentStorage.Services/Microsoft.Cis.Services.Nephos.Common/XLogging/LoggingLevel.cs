using System;

namespace Microsoft.Cis.Services.Nephos.Common.XLogging
{
	[Flags]
	public enum LoggingLevel
	{
		None = 0,
		Delete = 2,
		Write = 4,
		Read = 8,
		Critical = 16384
	}
}