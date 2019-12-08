using Microsoft.Cis.Services.Nephos.Common.Logging;
using System;

namespace Microsoft.Cis.Services.Nephos.Common
{
	public abstract class Logger : Logger<IRestProtocolHeadLogger>
	{
		protected Logger()
		{
		}
	}
}