using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public enum EmulatorErrorCode
	{
		NeedToWaitForAccountDeletion = -18,
		CannotDeleteDefaultAccount = -17,
		AccountNotFound = -16,
		AccountAlreadyExists = -15,
		FailedToClearData = -14,
		DatabaseCreationFailed = -13,
		ReservationFailedIncorrectUser = -12,
		ReservationFailed = -11,
		NoSqlInstanceFound = -10,
		UserSpecifiedSqlInstanceNotFound = -9,
		InitializationRequired = -8,
		CorruptInstallation = -7,
		AlreadyStopped = -6,
		AlreadyRunning = -5,
		StopRequired = -4,
		StartFailed = -3,
		CommandLineParsingFailed = -2,
		UnknownError = -1,
		Success = 0
	}
}