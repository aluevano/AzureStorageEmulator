using Microsoft.Cis.Services.Nephos.Common.Storage;
using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Storage.Service.ServiceManager
{
	public class BlobCopyInformationResult : IBlobCopyInformationResult
	{
		public IBlobCopyInformation CopyInformation
		{
			get;
			set;
		}

		public IBlobProperties Properties
		{
			get;
			set;
		}

		public BlobCopyInformationResult()
		{
		}
	}
}