namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface IFrontEndGeneralEventLogger
	{
		[EventStream]
		IFrontEndGeneralEventsStream General
		{
			get;
		}
	}
}