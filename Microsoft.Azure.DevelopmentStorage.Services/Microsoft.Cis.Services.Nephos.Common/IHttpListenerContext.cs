using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public interface IHttpListenerContext
	{
		IHttpListenerRequest Request
		{
			get;
		}

		IHttpListenerResponse Response
		{
			get;
		}

		IntPtr GetQueueHandle();
	}
}