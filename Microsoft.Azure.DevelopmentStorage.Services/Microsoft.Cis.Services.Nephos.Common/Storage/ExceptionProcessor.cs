using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class ExceptionProcessor
	{
		public ExceptionProcessor()
		{
		}

		public static bool LogUnhandledException(Exception e)
		{
			IStringDataEventStream error = Logger<IRestProtocolHeadLogger>.Instance.Error;
			object[] str = new object[] { e.ToString() };
			error.Log("[FEUnhandledException] Process crashed due to fatal exception: {0}", str);
			return true;
		}
	}
}