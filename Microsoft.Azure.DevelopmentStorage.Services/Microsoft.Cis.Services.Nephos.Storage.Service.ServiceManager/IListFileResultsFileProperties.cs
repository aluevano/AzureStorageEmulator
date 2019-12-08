using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IListFileResultsFileProperties
	{
		string ContainerName
		{
			get;
		}

		long? ContentLength
		{
			get;
		}

		string FileOrDirectoryName
		{
			get;
		}

		bool IsFile
		{
			get;
		}
	}
}