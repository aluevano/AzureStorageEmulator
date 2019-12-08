using System;
using System.Runtime.CompilerServices;

namespace Microsoft.Cis.Services.Nephos.Common.Storage
{
	public class CopyBlobOperationInfo
	{
		public Guid CopyId
		{
			get;
			set;
		}

		public string CopyStatus
		{
			get;
			set;
		}

		public CopyBlobOperationInfo()
		{
		}
	}
}