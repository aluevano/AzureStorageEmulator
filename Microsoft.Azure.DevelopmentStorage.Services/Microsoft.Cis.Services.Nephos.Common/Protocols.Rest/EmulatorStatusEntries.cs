using System;
using System.Net;

namespace Microsoft.Cis.Services.Nephos.Common.Protocols.Rest
{
	public static class EmulatorStatusEntries
	{
		public readonly static NephosStatusEntry VersionNotSupportedByEmulator;

		public readonly static NephosStatusEntry FeatureNotSupportedByEmulator;

		static EmulatorStatusEntries()
		{
			EmulatorStatusEntries.VersionNotSupportedByEmulator = new NephosStatusEntry("VersionNotSupportedByEmulator", HttpStatusCode.BadRequest, "The REST version of this request is not supported by this release of the Storage Emulator. Please upgrade the storage emulator to the latest version. Refer to the following URL for more information: http://go.microsoft.com/fwlink/?LinkId=392237");
			EmulatorStatusEntries.FeatureNotSupportedByEmulator = new NephosStatusEntry("FeatureNotSupportedByEmulator", HttpStatusCode.BadRequest, "This feature is not currently supported by the Storage Emulator.");
		}
	}
}