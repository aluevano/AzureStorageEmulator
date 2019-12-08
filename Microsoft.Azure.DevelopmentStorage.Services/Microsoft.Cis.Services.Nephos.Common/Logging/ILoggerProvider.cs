using System;

namespace Microsoft.Cis.Services.Nephos.Common.Logging
{
	public interface ILoggerProvider
	{
		Guid CreateActivityId(bool useDiscriminator);

		object GetLogger(Type loggerType, string name);
	}
}