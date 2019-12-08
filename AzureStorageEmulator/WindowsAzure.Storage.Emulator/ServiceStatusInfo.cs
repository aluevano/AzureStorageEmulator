using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator
{
	internal class ServiceStatusInfo
	{
		public Exception Error
		{
			get;
			set;
		}

		public bool IsRunning
		{
			get;
			set;
		}

		public string ServiceEndpoint
		{
			get;
			set;
		}

		public string ServiceName
		{
			get;
			set;
		}

		public ServiceStatusInfo()
		{
		}
	}
}