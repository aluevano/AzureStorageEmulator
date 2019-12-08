using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface ISetBlobPropertiesResult
	{
		DateTime LastModifiedTime
		{
			get;
		}

		long? SequenceNumber
		{
			get;
		}
	}
}