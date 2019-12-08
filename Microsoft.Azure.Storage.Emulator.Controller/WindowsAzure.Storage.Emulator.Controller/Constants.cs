using System;

namespace Microsoft.WindowsAzure.Storage.Emulator.Controller
{
	public static class Constants
	{
		public const string StorageEmulatorStartupEventName = "DevelopmentStorage-7580AFBB-2BEC-4269-B083-46C1361A69B0";

		public const string StorageEmulatorFailureEventName = "DevelopmentStorage-Failure-7580AFBB-2BEC-4269-B083-46C1361A69B0";

		public const string StorageEmulatorShutdownEventName = "DevelopmentStorage-3D75486A-E34F-447c-BF4B-A35284FA8D96";

		public const string StorageEmulatorUserInterfaceEventName = "DevelopmentStorage-62F76D03-83B4-40ac-A100-F990EA19CF99";

		public const string BlobService = "Blob";

		public const string QueueService = "Queue";

		public const string TableService = "Table";

		public const string BannerVersion = "5.2.0.0";

		public const string MainHelpLink = "http://go.microsoft.com/fwlink/?LinkId=392237";

		public const string CommandLineHelpLink = "http://go.microsoft.com/fwlink/?LinkId=392235";

		internal const string DefaultSqlServerName = "localhost\\SQLExpress";

		internal const string RootKeyPathRegPath = "SOFTWARE\\Microsoft\\Windows Azure Storage Emulator";

		internal const string InstallPathKeyName = "InstallPath";

		internal const string MajorMinorEmulatorVersion = "5.2";

		public readonly static string EmulatorDBName;

		public readonly static TimeSpan AccountRecreationTimeLimit;

		internal readonly static string DeploymentBannerVersion;

		static Constants()
		{
			Constants.AccountRecreationTimeLimit = TimeSpan.FromSeconds(30);
			char[] chrArray = new char[] { '.' };
			Constants.DeploymentBannerVersion = string.Concat("5.2".Split(chrArray));
			Constants.EmulatorDBName = string.Format("AzureStorageEmulatorDb{0}", Constants.DeploymentBannerVersion);
		}
	}
}