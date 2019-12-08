using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	[Flags]
	public enum EmulatorServiceType
	{
		None = 0,
		Blob = 1,
		Queue = 2,
		Table = 4,
		All = 255
	}
}