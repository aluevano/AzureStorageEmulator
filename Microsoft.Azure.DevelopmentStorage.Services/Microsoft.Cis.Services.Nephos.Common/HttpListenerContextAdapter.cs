using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public class HttpListenerContextAdapter : IHttpListenerContext
	{
		private HttpListenerContext httpListenerContext;

		private HttpListenerRequestAdapter httpListenerRequestAdapter;

		private HttpListenerResponseAdapter httpListenerResponseAdapter;

		private static PropertyInfo RequestQueueHandlePropertyInfo;

		public IHttpListenerRequest Request
		{
			get
			{
				if (this.httpListenerRequestAdapter == null)
				{
					this.httpListenerRequestAdapter = new HttpListenerRequestAdapter(this.httpListenerContext.Request);
				}
				return this.httpListenerRequestAdapter;
			}
		}

		public IHttpListenerResponse Response
		{
			get
			{
				if (this.httpListenerResponseAdapter == null)
				{
					this.httpListenerResponseAdapter = new HttpListenerResponseAdapter(this.httpListenerContext.Response);
				}
				return this.httpListenerResponseAdapter;
			}
		}

		static HttpListenerContextAdapter()
		{
			HttpListenerContextAdapter.RequestQueueHandlePropertyInfo = typeof(HttpListenerContext).GetProperty("RequestQueueHandle", BindingFlags.Instance | BindingFlags.NonPublic, null, typeof(CriticalHandle), Type.EmptyTypes, null);
			if (HttpListenerContextAdapter.RequestQueueHandlePropertyInfo == null)
			{
				Logger<IRestProtocolHeadLogger>.Instance.Error.Log("Could not find propertyName 'RequestQueueHandle' as part of the reflected properties");
			}
		}

		public HttpListenerContextAdapter(HttpListenerContext context)
		{
			this.httpListenerContext = context;
		}

		public IntPtr GetQueueHandle()
		{
			if (HttpListenerContextAdapter.RequestQueueHandlePropertyInfo == null)
			{
				throw new InvalidOperationException("Property info for 'RequestQueueHandle' property not found in the reflected properties of type 'HttpListenerContext'");
			}
			return (IntPtr)HttpListenerContextAdapter.RequestQueueHandlePropertyInfo.GetValue(this.httpListenerContext, null);
		}
	}
}