using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public interface IBlobCopyInformationResult
	{
		IBlobCopyInformation CopyInformation
		{
			get;
			set;
		}

		IBlobProperties Properties
		{
			get;
			set;
		}
	}
}