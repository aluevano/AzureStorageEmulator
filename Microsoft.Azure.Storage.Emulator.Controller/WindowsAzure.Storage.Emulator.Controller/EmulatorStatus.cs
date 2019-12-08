using System;
using System.Runtime.CompilerServices;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public class EmulatorStatus
	{
		public string BlobEndpoint
		{
			get;
			set;
		}

		public bool IsRunning
		{
			get;
			set;
		}

		public string QueueEndpoint
		{
			get;
			set;
		}

		public string TableEndpoint
		{
			get;
			set;
		}

		public EmulatorStatus()
		{
		}
	}
}