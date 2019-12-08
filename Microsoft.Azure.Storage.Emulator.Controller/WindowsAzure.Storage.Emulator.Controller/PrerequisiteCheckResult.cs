using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	internal enum PrerequisiteCheckResult
	{
		SqlExpressInstanceNotInstalledOrRunning,
		SqlInstanceRunning,
		DatabaseNotInstalledOrLoginFailed,
		DatabaseInstalled,
		PortsNotReserved,
		NoDatabaseAccess,
		HasDatabaseAccess,
		None
	}
}